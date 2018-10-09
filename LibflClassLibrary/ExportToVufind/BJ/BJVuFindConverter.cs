﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using System.Drawing;
using System.Windows.Forms;
using Utilities;
using LibflClassLibrary.ExportToVufind.Vufind;
using LibflClassLibrary.Books.BJBooks.DB;
using LibflClassLibrary.Books.BJBooks;
using LibflClassLibrary.Books.BJBooks.BJExemplars;
using System.Security.Cryptography;
using System.Drawing.Imaging;
using System.Security.AccessControl;

namespace LibflClassLibrary.ExportToVufind.BJ
{
    public class BJVuFindConverter : VuFindConverter
    {

        public BJVuFindConverter(string fund)
        {
            this.Fund = fund;
            this.dbWrapper = new BJDatabaseWrapper(fund);
        }

        private int _lastID = 1;
        private BJDatabaseWrapper dbWrapper;
        private List<string> errors = new List<string>();
        private VufindXMLWriter writer;
        public override void Export()
        {
            writer = new VufindXMLWriter(this.Fund);
            writer.StartVufindXML(@"F:\import\" + Fund.ToLower() + ".xml");
            /////////////////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////BJVVV/////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////////////////////////
            _lastID = 1;
            this.StartExportFrom( _lastID );
        }

        private void StartExportFrom( int previous )
        {
            int step = 1;
            int MaxIDMAIN = GetMaxIDMAIN();
            VufindDoc vfDoc = new VufindDoc();
            for (int i = previous; i < MaxIDMAIN; i += step)
            {
                _lastID = i;

                
                DataTable record = dbWrapper.GetBJRecord(_lastID);
                if (record.Rows.Count == 0) continue; //если сводный уровень, то пропускаем пока. тут ещё может пин не существовать
                try
                {
                    vfDoc = CreateVufindDoc( record );
                    if (vfDoc == null) continue;//одна из причин - все экземпляры списаны и нет электронного экземпляра
                }
                catch (Exception ex)
                {
                    errors.Add(this.Fund + "_" + i);
                    continue;
                }

                writer.AppendVufindDoc(vfDoc);

                VuFindConverterEventArgs args = new VuFindConverterEventArgs();
                args.RecordId = this.Fund + "_" + i;
                OnRecordExported(args);
            }
            writer.FinishWriting();
            File.WriteAllLines(@"f:\import\importErrors\" + this.Fund + "Errors.txt", errors.ToArray());

        }
        public override void ExportSingleRecord( int idmain )
        {
            VufindXMLWriter writer = new VufindXMLWriter(this.Fund);
            VufindDoc vfDoc = new VufindDoc();

            DataTable record = dbWrapper.GetBJRecord(idmain); 
            vfDoc = CreateVufindDoc(record);
            if (vfDoc == null)
            {
                MessageBox.Show("Запись не имеет экземпляров.");
                return;
            }
            writer.WriteSingleRecord(vfDoc);
            MessageBox.Show("Завершено");
        }


        private bool SpecialFilter(int IDMAIN)
        {
            if ((IDMAIN == 52109) && (Fund == "REDKOSTJ"))
            {
                //эта запись образец заполнения электронной копии
                return false;
            }
            if ((IDMAIN == 1456705) && (Fund == "BJVVV"))
            {
                //эта запись образец заполнения электронной копии
                return false;
            }

            return true;
        }

        public VufindDoc CreateVufindDoc( DataTable BJBook )
        {
            int currentIDMAIN = (int)BJBook.Rows[0]["IDMAIN"];
            if (!SpecialFilter(currentIDMAIN)) return null;
            string level = BJBook.Rows[0]["Level"].ToString();
            string level_id = BJBook.Rows[0]["level_id"].ToString();
            int lev_id = int.Parse(level_id);
            if (lev_id < 0) return null;
            StringBuilder allFields = new StringBuilder();
            AuthoritativeFile AF_all = new AuthoritativeFile();
            bool wasTitle = false;//встречается ошибка: два заглавия в одном пине
            bool wasAuthor = false;//был ли автор. если был, то сортировочное поле уже заполнено
            string description = "";//все 3хх поля
            DataTable clarify;
            string Annotation = "";
            int CarrierCode;
            VufindDoc result = new VufindDoc();

#region field analyse
            //BJBookInfo book = new BJBookInfo();
            foreach (DataRow r in BJBook.Rows)
            {

                allFields.AppendFormat(" {0}", r["PLAIN"].ToString());
                switch (r["code"].ToString())
                {
                    //=======================================================================Родные поля вуфайнд=======================
                    case "200$a":
                        if (wasTitle) break;
                        result.title.Add(r["PLAIN"].ToString());
                        result.title_short.Add(r["PLAIN"].ToString());
                        result.title_sort.Add(r["SORT"].ToString());
                        wasTitle = true;
                        break;
                    case "700$a":
                        result.author.Add(r["PLAIN"].ToString());
                        if (!wasAuthor)
                        {
                            //AddField("author_sort", r["SORT"].ToString());
                            result.author_sort.Add(r["SORT"].ToString());
                        }
                        wasAuthor = true;
                        //забрать все варианты написания автора из авторитетного файла и вставить в скрытое, но поисковое поле
                        break;
                    case "701$a":
                        result.author2.Add(r["PLAIN"].ToString());
                        break;
                    case "710$a":
                        result.author_corporate.Add(r["PLAIN"].ToString());
                        break;
                    case "710$4":
                        result.author_corporate_role.Add(r["PLAIN"].ToString());
                        break;
                    case "700$4":
                        result.author_role.Add(r["PLAIN"].ToString());
                        break;
                    case "701$4":
                        result.author2_role.Add(r["PLAIN"].ToString());
                        break;
                    case "921$a":
                        CarrierCode = KeyValueMapping.CarrierNameToCode.GetValueOrDefault(r["PLAIN"].ToString(), 3001);
                        result.format.Add(CarrierCode.ToString());
                        break;
                    case "922$e":
                        result.genre.Add(r["PLAIN"].ToString());
                        result.genre_facet.Add(r["PLAIN"].ToString());
                        break;
                    case "10$a":
                        clarify = dbWrapper.Clarify_10a((int)r["IDDATA"]);
                        string add = r["PLAIN"].ToString();
                        if (clarify.Rows.Count != 0)
                        {
                            add = r["PLAIN"].ToString() + " (" + clarify.Rows[0]["PLAIN"].ToString() + ")";
                        }
                        result.isbn.Add(add);
                        break;
                    case "11$a":
                        result.issn.Add(r["PLAIN"].ToString());
                        break;
                    case "101$a":
                        clarify = dbWrapper.Clarify_101a((int)r["IDINLIST"]);
                        if (clarify.Rows.Count == 0)
                        {
                            result.language.Add(r["PLAIN"].ToString());
                        }
                        else
                        {
                            result.language.Add(clarify.Rows[0]["NAME"].ToString());
                        }
                        break;
                    case "2100$d":
                        result.publishDate.Add(r["PLAIN"].ToString());
                        break;
                    case "210$c":
                        result.publisher.Add(r["PLAIN"].ToString());
                        break;
                    case "517$a":
                        clarify = dbWrapper.Clarify_517a((int)r["IDDATA"]);
                        string fieldValue;
                        fieldValue = (clarify.Rows.Count != 0) ?
                            "(" + clarify.Rows[0]["PLAIN"].ToString() + ")" + r["PLAIN"].ToString() :
                            r["PLAIN"].ToString();

                        result.title_alt.Add(fieldValue);
                        //нужно специальным образом обрабатывать
                        break;
                    //=======================================================================добавленные поля в Вуфайнд=======================
                    case "210$a":
                        result.PlaceOfPublication.Add(r["PLAIN"].ToString());
                        break;
                    case "200$6":
                        result.Title_another_chart.Add(r["PLAIN"].ToString());
                        break;
                    case "200$b":
                        result.Title_same_author.Add(r["PLAIN"].ToString());
                        break;
                    case "200$d":
                        result.Parallel_title.Add(r["PLAIN"].ToString());
                        break;
                    case "200$e":
                        result.Info_pertaining_title.Add(r["PLAIN"].ToString());
                        break;
                    case "200$f":
                        result.Responsibility_statement.Add(r["PLAIN"].ToString());
                        break;
                    case "200$h":
                        result.Part_number.Add(r["PLAIN"].ToString());
                        break;
                    case "200$i":
                        result.Part_title.Add(r["PLAIN"].ToString());
                        break;
                    case "200$z":
                        result.Language_title_alt.Add(r["PLAIN"].ToString());
                        break;
                    case "500$a":
                        result.Title_unified.Add(r["PLAIN"].ToString());
                        break;
                    case "500$3"://$3 is deprecated!!!
                        AF_all = GetAFAll( (int)r["AFLINKID"], "AFHEADERVAR");
                        result.Title_unified.Add(AF_all.ToString());
                        break;
                    case "517$e":
                        result.Info_title_alt.Add(r["PLAIN"].ToString());
                        break;
                    case "517$z":
                        result.Language_title_alt.Add(r["PLAIN"].ToString());
                        break;
                    case "700$3":
                        AF_all = GetAFAll((int)r["AFLINKID"], "AFNAMESVAR");
                        foreach (string av in AF_all.AFValues)
                        {
                            result.author_variant.Add(av);
                        }
                        break;
                    case "701$3":
                        AF_all = GetAFAll((int)r["AFLINKID"], "AFNAMESVAR");
                        result.Another_author_AF_all.Add(AF_all.ToString());//хранить но не отображать
                        break;
                    case "501$a":
                        result.Another_title.Add(r["PLAIN"].ToString());
                        break;
                    case "501$3":
                        AF_all = GetAFAll((int)r["AFLINKID"], "AFHEADERVAR");
                        result.Another_title_AF_All.Add(AF_all.ToString());
                        break;
                    case "503$a":
                        result.Unified_Caption.Add(r["PLAIN"].ToString());
                        break;
                    case "503$3":
                        AF_all = GetAFAll((int)r["AFLINKID"], "AFHEADERVAR");
                        result.Unified_Caption_AF_All.Add(AF_all.ToString());
                        break;
                    case "700$6":
                        result.Author_another_chart.Add(r["PLAIN"].ToString());
                        break;
                    case "702$a":
                        result.Editor.Add(r["PLAIN"].ToString());
                        break;
                    case "702$3":
                        AF_all = GetAFAll((int)r["AFLINKID"], "AFNAMESVAR");
                        result.Editor_AF_all.Add(AF_all.ToString());
                        break;
                    case "702$4":
                        result.Editor_role.Add(r["PLAIN"].ToString());
                        break;
                    case "710$3":
                        AF_all = GetAFAll((int)r["AFLINKID"], "AFORGSVAR");
                        result.Collective_author_all.Add(AF_all.ToString());
                        break;
                    case "710$9":
                        result.Organization_nature.Add(r["PLAIN"].ToString());
                        break;
                    case "11$9":
                        result.Printing.Add(r["PLAIN"].ToString());
                        break;
                    case "205$a":
                        string PublicationInfo = r["PLAIN"].ToString();

                        // 205$b
                         
                        clarify = dbWrapper.Clarify_205a_1((int)r["IDDATA"]);
                        foreach (DataRow rr in clarify.Rows)
                        {
                            PublicationInfo += "; " + rr["PLAIN"].ToString();
                        }
                        // 205$f
                        clarify = dbWrapper.Clarify_205a_2((int)r["IDDATA"]);
                        foreach (DataRow rr in clarify.Rows)
                        {
                            PublicationInfo += " / " + rr["PLAIN"].ToString();
                        }
                        // 205$g
                        clarify = dbWrapper.Clarify_205a_3((int)r["IDDATA"]);
                        foreach (DataRow rr in clarify.Rows)
                        {
                            PublicationInfo += "; " + rr["PLAIN"].ToString();
                        }
                        result.Publication_info.Add(PublicationInfo);
                        break;
                    case "921$b":
                        result.EditionType.Add(r["PLAIN"].ToString());
                        break;
                    case "102$a":
                        result.Country.Add(r["PLAIN"].ToString());
                        break;
                    case "210$3":
                        AF_all = GetAFAll((int)r["AFLINKID"], "AFORGSVAR");
                        result.PlaceOfPublication_AF_All.Add(AF_all.ToString());
                        break;
                    case "2110$g":
                        result.PrintingHouse.Add(r["PLAIN"].ToString()); 
                        break;
                    case "2110$3":
                        AF_all = GetAFAll((int)r["AFLINKID"], "AFORGSVAR");
                        result.PrintingHouse_AF_All.Add(AF_all.ToString());
                        break;
                    case "2111$e":
                        result.GeoNamePlaceOfPublication.Add(r["PLAIN"].ToString());
                        break;
                    case "2111$3":
                        AF_all = GetAFAll((int)r["AFLINKID"], "AFGEOVAR");
                        result.GeoNamePlaceOfPublication_AF_All.Add(AF_all.ToString());
                        break;
                    case "10$z":
                        result.IncorrectISBN.Add(r["PLAIN"].ToString()); 
                        break;
                    case "11$z":
                        result.IncorrectISSN.Add(r["PLAIN"].ToString());
                        break;
                    case "11$y":
                        result.CanceledISSN.Add(r["PLAIN"].ToString());
                        break;
                    case "101$b":
                        result.IntermediateTranslateLanguage.Add(r["PLAIN"].ToString());
                        break;
                    case "101$d":
                        result.SummaryLanguage.Add(r["PLAIN"].ToString());
                        break;
                    case "101$e":
                        result.TableOfContentsLanguage.Add(r["PLAIN"].ToString()); 
                        break;
                    case "101$f":
                        result.TitlePageLanguage.Add(r["PLAIN"].ToString());
                        break;
                    case "101$g":
                        result.BasicTitleLanguage.Add(r["PLAIN"].ToString()); 
                        break;
                    case "101$i":
                        result.AccompayingMaterialLanguage.Add(r["PLAIN"].ToString());
                        break;
                    case "215$a":
                        result.Volume.Add(r["PLAIN"].ToString());
                        break;
                    case "215$b":
                        result.Illustrations.Add(r["PLAIN"].ToString());
                        break;
                    case "215$c":
                        result.Dimensions.Add(r["PLAIN"].ToString()); 
                        break;
                    case "215$d":
                        result.AccompayingMaterial.Add(r["PLAIN"].ToString()); 
                        break;
                    case "225$a":
                        if (r["PLAIN"].ToString() == "") break;
                        if (r["PLAIN"].ToString() == "-1") break;
                        //AddHierarchyFields(Convert.ToInt32(r["PLAIN"]), Convert.ToInt32(r["IDMAIN"]), result);
                        break;
                    case "225$h":
                        result.NumberInSeries.Add(r["PLAIN"].ToString()); 
                        break;
                    case "225$v":
                        result.NumberInSubseries.Add(r["PLAIN"].ToString()); 
                        break;
                    case "300$a":
                    case "301$a":
                    case "316$a":
                    case "320$a":
                    case "326$a":
                    case "336$a":
                    case "337$a":
                        description += r["PLAIN"].ToString() + " ; ";
                        break;
                    case "327$a":
                    case "330$a":
                        Annotation += r["PLAIN"].ToString() + " ; ";
                        break;
                    case "830$a":
                        result.CatalogerNote.Add(r["PLAIN"].ToString());
                        break;
                    case "831$a":
                        result.DirectoryNote.Add(r["PLAIN"].ToString());
                        break;
                    case "924$a":
                        result.AdditionalBibRecord.Add(r["PLAIN"].ToString());
                        break;
                    case "940$a":
                        result.HyperLink.Add(r["PLAIN"].ToString()); 
                        break;
                    case "606$a"://"""""" • """"""
                        int IDChain = 0;
                        string row = r["SORT"].ToString();
                        if (!int.TryParse(row, out IDChain))
                        {
                            continue;
                        }
                        clarify = dbWrapper.Clarify_606a(IDChain);
                        if (clarify.Rows.Count == 0) break;
                        string TPR = "";
                        foreach (DataRow rr in clarify.Rows)
                        {
                            TPR += rr["VALUE"].ToString() + " • ";
                        }
                        TPR = TPR.Substring(0, TPR.Length - 2);
                        result.topic.Add(TPR);
                        result.topic_facet.Add(TPR);
                        break;
                    case "3000$a":
                        result.OwnerPerson.Add(r["PLAIN"].ToString());
                        break;
                    case "3000$3":
                        AF_all = GetAFAll((int)r["AFLINKID"], "AFNAMESVAR");
                        result.OwnerPerson_AF_All.Add(AF_all.ToString());
                        break;
                    case "3001$a":
                        result.OwnerOrganization.Add(r["PLAIN"].ToString());
                        break;
                    case "3001$3":
                        AF_all = GetAFAll((int)r["AFLINKID"], "AFORGSVAR");
                        result.OwnerOrganization_AF_All.Add(AF_all.ToString());
                        break;
                    case "3002$a":
                        result.Ownership.Add(r["PLAIN"].ToString()); 
                        break;
                    case "3003$a":
                        result.OwnerExemplar.Add(r["PLAIN"].ToString());
                        break;
                    case "3200$a":
                        result.IllustrationMaterial.Add(r["PLAIN"].ToString());
                        break;
                }

            }
#endregion
            result.id = this.Fund + "_" + currentIDMAIN;
            string rusFund = GetFundId(this.Fund);

            result.fund = rusFund;
            result.allfields = allFields.ToString();
            result.Level = level;
            result.Level_id = level_id;
            result.Annotation.Add(Annotation);

            if (description != "")
            {
                result.description.Add(description);
            }
            if (int.Parse(result.Level_id) < 0)
            {
                return result;
            }
            AddExemplarFields(currentIDMAIN, result, this.Fund);

            if ((result.Exemplars.Count == 0))//все экземпляры отсеялись сами собой.
            {
                return null;
            }

            return result;
        }


        private void AddExemplarFields(int idmain, VufindDoc result, string fund)
        {

            DataTable table = dbWrapper.GetIdDataOfAllExemplars(idmain);
            if (table.Rows.Count == 0)
            {
                DataTable IsElectronicCopyExists = dbWrapper.GetHyperLink(idmain);
                if (IsElectronicCopyExists.Rows.Count == 0)
                {
                    return;//все списано и нет электронных копий.
                }
            }

            int IDMAIN = idmain;//(int)table.Rows[0]["IDMAIN"];

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            JsonWriter writer = new JsonTextWriter(sw);

            //3901 - проверить, почему у этого пина удаленный доступ

            //{"1":
            //    {
            //     "exemplar_location":"Абонемент",
            //     "exemplar_collection":"ОФ",
            //     "exemplar_placing_cipher":"08.B 4650",
            //     "exemplar_carrier":"Бумага",
            //     "exemplar_inventory_number":"2494125",
            //     "exemplar_class_edition":"Для выдачи"
            //    },
            // "2":
            //    {
            //        "exemplar_location":"Абонемент",
            //        "exemplar_collection":"ОФ",
            //        "exemplar_placing_cipher":"08.B 4651",
            //        "exemplar_carrier":"Бумага",
            //        "exemplar_inventory_number":"2494126",
            //        "exemplar_class_edition":"Для выдачи"
            //    }
            //}


            writer.WriteStartObject();

            DataTable exemplar;
            int cnt = 1;
            string CarrierCode = "";
            List<DateTime> ExemplarsCreatedDateList = new List<DateTime>();

            foreach (DataRow iddata in table.Rows)
            {
                exemplar = dbWrapper.GetExemplar((int)iddata["IDDATA"]);
                ExemplarInfo bjExemplar = ExemplarInfo.GetExemplarByIdData((int)iddata["IDDATA"], this.Fund);
                if (bjExemplar.ExemplarAccess.Access == 1020)//экстремистская литература. не выгружаем такое.
                {
                    continue;
                }

                writer.WritePropertyName(cnt++.ToString());
                writer.WriteStartObject();

                //ExemplarInfo bjExemplar = new ExemplarInfo((int)iddata["IDDATA"]);
                ExemplarInfo Convolute = null;
                #region FieldAnalyse
                foreach (DataRow r in exemplar.Rows)
                {
                    string code = r["MNFIELD"].ToString() + r["MSFIELD"].ToString();
                    switch (code)
                    {
                        case "899$a":
                            string plain = r["PLAIN"].ToString();
                            string check = r["NAME"].ToString();
                            string UL = KeyValueMapping.UnifiedLocation.GetValueOrDefault(r["NAME"].ToString(), "отсутствует в словаре");
                            int LocationCode = KeyValueMapping.UnifiedLocationCode.GetValueOrDefault(UL, 2999);

                            writer.WritePropertyName("exemplar_location");
                            writer.WriteValue(LocationCode);
                            result.Location.Add(LocationCode.ToString());
                            break;
                        case "482$a":
                            writer.WritePropertyName("exemplar_interlaced_to");
                            writer.WriteValue(r["PLAIN"].ToString());
                            
                            writer.WritePropertyName("exemplar_convolute");
                            Convolute = ExemplarInfo.GetExemplarByInventoryNumber(r["PLAIN"].ToString(), this.Fund);
                            if (Convolute == null)
                            {
                                writer.WriteValue("Ошибка заполнения библиографического описания конволюта");
                                errors.Add(this.Fund + "_" + idmain);
                            }
                            else
                            {
                                writer.WriteValue(this.Fund + "_"+Convolute.IDMAIN);
                            }
                            break;
                        case "899$b"://тут надо оставить только коллекции
                            string fnd = r["PLAIN"].ToString();
                            writer.WritePropertyName("exemplar_collection");
                            writer.WriteValue(r["PLAIN"].ToString());
                            break;
                        case "899$c":
                            writer.WritePropertyName("exemplar_rack_location");
                            writer.WriteValue(r["PLAIN"].ToString());
                            break;
                        case "899$d":
                            writer.WritePropertyName("exemplar_direction_temporary_storage");
                            writer.WriteValue(r["PLAIN"].ToString());
                            break;
                        case "899$j":
                            result.PlacingCipher.Add(r["PLAIN"].ToString());
                            writer.WritePropertyName("exemplar_placing_cipher");
                            writer.WriteValue(r["PLAIN"].ToString());
                            break;
                        case "899$p":
                            writer.WritePropertyName("exemplar_inventory_number");
                            writer.WriteValue(r["PLAIN"].ToString());
                            ExemplarsCreatedDateList.Add((DateTime)r["Created"]);
                            break;
                        case "899$w":
                            writer.WritePropertyName("exemplar_barcode");
                            writer.WriteValue(r["PLAIN"].ToString());
                            ExemplarsCreatedDateList.Add((DateTime)r["Created"]);
                            break;
                        case "899$x":
                            writer.WritePropertyName("exemplar_inv_note");
                            writer.WriteValue(r["PLAIN"].ToString());
                            break;
                        case "921$a":
                            writer.WritePropertyName("exemplar_carrier");
                            CarrierCode = KeyValueMapping.CarrierNameToCode.GetValueOrDefault(r["PLAIN"].ToString(), 3001).ToString();
                            writer.WriteValue(CarrierCode);
                            break;
                        case "921$c":
                            writer.WritePropertyName("exemplar_class_edition");
                            writer.WriteValue(r["PLAIN"].ToString());
                            break;
                        case "921$d":

                            break;
                        //case "922$b":
                            //Exemplar += "Трофей\\Принадлежность к:" + r["PLAIN"].ToString() + "#";
                            //writer.WritePropertyName("exemplar_trophy");
                           // writer.WriteValue(r["PLAIN"].ToString());
                           // break;
                        case "3300$a":
                            writer.WritePropertyName("exemplar_binding_kind");
                            writer.WriteValue(r["PLAIN"].ToString());
                            break;
                        case "3300$b":
                            writer.WritePropertyName("exemplar_binding_age");
                            writer.WriteValue(r["PLAIN"].ToString());
                            break;
                        case "3300$c":
                            writer.WritePropertyName("exemplar_binding_type");
                            writer.WriteValue(r["PLAIN"].ToString());
                            break;
                        case "3300$d":
                            writer.WritePropertyName("exemplar_cover_material");
                            writer.WriteValue(r["PLAIN"].ToString());
                            break;
                        case "3300$e":
                            writer.WritePropertyName("exemplar_material_on_cover");
                            writer.WriteValue(r["PLAIN"].ToString());
                            break;
                        case "3300$f":
                            writer.WritePropertyName("exemplar_spine_material");
                            writer.WriteValue(r["PLAIN"].ToString());
                            break;
                        case "3300$g":
                            writer.WritePropertyName("exemplar_bandages");
                            writer.WriteValue(r["PLAIN"].ToString());
                            break;
                        case "3300$h":
                            writer.WritePropertyName("exemplar_stamping_on_cover");
                            writer.WriteValue(r["PLAIN"].ToString());
                            break;
                        case "3300$i":
                            writer.WritePropertyName("exemplar_embossing_on_spine");
                            writer.WriteValue(r["PLAIN"].ToString());
                            break;
                        case "3300$j":
                            writer.WritePropertyName("exemplar_fittings");
                            writer.WriteValue(r["PLAIN"].ToString());
                            break;
                        case "3300$k":
                            writer.WritePropertyName("exemplar_bugs");
                            writer.WriteValue(r["PLAIN"].ToString());
                            break;
                        case "3300$l":
                            writer.WritePropertyName("exemplar_forth");
                            writer.WriteValue(r["PLAIN"].ToString());
                            break;
                        case "3300$m":
                            writer.WritePropertyName("exemplar_cutoff");
                            writer.WriteValue(r["PLAIN"].ToString());
                            break;
                        case "3300$n":
                            writer.WritePropertyName("exemplar_condition");
                            writer.WriteValue(r["PLAIN"].ToString());
                            break;
                        case "3300$o":
                            writer.WritePropertyName("exemplar_case");
                            writer.WriteValue(r["PLAIN"].ToString());
                            break;
                        case "3300$p":
                            writer.WritePropertyName("exemplar_embossing_on_edge");
                            writer.WriteValue(r["PLAIN"].ToString());
                            break;
                        case "3300$r":
                            writer.WritePropertyName("exemplar_binding_note");
                            writer.WriteValue(r["PLAIN"].ToString());
                            break;
                    }

                }
                #endregion
                //Exemplar += "exemplar_id:" + ds.Tables["t"].Rows[0]["IDDATA"].ToString() + "#";
                writer.WritePropertyName("exemplar_id");
                writer.WriteValue(iddata["IDDATA"].ToString());


                result.MethodOfAccess.Add(bjExemplar.ExemplarAccess.MethodOfAccess.ToString());
                writer.WritePropertyName("exemplar_access");
                writer.WriteValue(bjExemplar.ExemplarAccess.Access);
                writer.WritePropertyName("exemplar_access_group");
                writer.WriteValue(KeyValueMapping.AccessCodeToGroup[bjExemplar.ExemplarAccess.Access]);
                result.Exemplars.Add(bjExemplar);

                writer.WriteEndObject();
            }

            //смотрим есть ли гиперссылка
            table = dbWrapper.GetHyperLink(IDMAIN);
            if (table.Rows.Count != 0)//если есть - вставляем отдельным экземпляром. после электронной инвентаризации этот кусок можно будет убрать
            {
                //ExemplarInfo bjExemplar = new ExemplarInfo(-1);
                writer.WritePropertyName(cnt++.ToString());
                writer.WriteStartObject();


                //Exemplar += "Электронная копия: есть#";
                writer.WritePropertyName("exemplar_electronic_copy");
                writer.WriteValue("да");
                //Exemplar += "Гиперссылка: " + ds.Tables["t"].Rows[0]["PLAIN"].ToString() + " #";
                writer.WritePropertyName("exemplar_hyperlink");
                writer.WriteValue(table.Rows[0]["PLAIN"].ToString());

                writer.WritePropertyName("exemplar_hyperlink_newviewer");
                writer.WriteValue("/Bookreader/Viewer?bookID="+result.id+"&view_mode=HQ");
                result.HyperLinkNewViewer.Add("/Bookreader/Viewer?bookID=" + result.id + "&view_mode=HQ");

                DataTable hyperLinkTable = dbWrapper.GetBookScanInfo(IDMAIN);
                bool ForAllReader = false;
                if (hyperLinkTable.Rows.Count != 0)
                {
                    ForAllReader = (bool)hyperLinkTable.Rows[0]["ForAllReader"];
                    //Exemplar += "Авторское право: " + ((ds.Tables["t"].Rows[0]["ForAllReader"].ToString() == "1") ? "нет" : "есть");
                    writer.WritePropertyName("exemplar_copyright");
                    writer.WriteValue( (ForAllReader) ? "нет" : "есть" );
                    //Exemplar += "Ветхий оригинал: " + ((ds.Tables["t"].Rows[0]["OldBook"].ToString() == "1") ? "да" : "нет");
                    writer.WritePropertyName("exemplar_old_original");
                    writer.WriteValue(((hyperLinkTable.Rows[0]["OldBook"].ToString() == "1") ? "да" : "нет"));
                    //Exemplar += "Наличие PDF версии: " + ((ds.Tables["t"].Rows[0]["PDF"].ToString() == "1") ? "да" : "нет");
                    writer.WritePropertyName("exemplar_PDF_exists");
                    writer.WriteValue(((hyperLinkTable.Rows[0]["PDF"].ToString() == "1") ? "да" : "нет"));
                    //Exemplar += "Доступ: Заказать через личный кабинет";
                    writer.WritePropertyName("exemplar_access");
                    writer.WriteValue(
                        (ForAllReader) ?
                        "1001" : "1002");
                        //"Для прочтения онлайн необходимо перейти по ссылке" :
                        //"Для прочтения онлайн необходимо положить в корзину и заказать через личный кабинет");
                    writer.WritePropertyName("exemplar_access_group");
                    writer.WriteValue((ForAllReader) ? KeyValueMapping.AccessCodeToGroup[1001] : KeyValueMapping.AccessCodeToGroup[1002]);

                    writer.WritePropertyName("exemplar_carrier");
                    //writer.WriteValue("Электронная книга");
                    writer.WriteValue("3011");

                    writer.WritePropertyName("exemplar_id");
                    writer.WriteValue("ebook");//для всех у кого есть электронная копия. АПИ когда это значение встретит, сразу вернёт "доступно"
                    writer.WritePropertyName("exemplar_location");
                    writer.WriteValue("2030");

                }
                writer.WriteEndObject();
                result.MethodOfAccess.Add("4002");

                //определить структуру, в которую полностью запись ложится и здесь её проверять, чтобы правильно вычисляемые поля проставить.
            }
            writer.WriteEndObject();
            writer.Flush();
            writer.Close();
            ElectronicExemplarInfo ElExemplar = new ElectronicExemplarInfo(-1);//фейковый электронный экземпляр
            result.Exemplars.Add(ElExemplar);
            result.ExemplarsJSON = sb.ToString();
            if (ExemplarsCreatedDateList.Count != 0)
            {
                result.NewArrivals = ExemplarsCreatedDateList.Min();
            }
        }


        public override void ExportCovers()
        {
            StringBuilder sb = new StringBuilder();
            DataTable table = dbWrapper.GetAllIdmainWithImages();
            //List<string> errors = new List<string>();
            int j = 0;
            foreach(DataRow row in table.Rows)
            {
                j++;
                int IDMAIN = (int)row["IDMAIN"];
                this.ExportSingleCover(IDMAIN);
                GC.Collect();
            }
            //File.WriteAllLines(@"f:\import\importErrors\" + this.Fund + "Errors.txt", errors.ToArray());

        }
        public override void ExportSingleCover(object idRecord)
        {
            //string ID = idRecord.ToString();
            //int IDMAIN = int.Parse(ID.Substring(ID.IndexOf("_")+1));
            int IDMAIN = (int)idRecord;

            string LoginPath = @"\\" + AppSettings.IPAddressFileServer + @"\BookAddInf";

            using (new NetworkConnection(LoginPath, new NetworkCredential(AppSettings.LoginFileServerReadWrite, AppSettings.PasswordFileServerReadWrite)))
            {
                string NewPath = VuFindConverter.GetCoverExportPath(this.Fund+"_"+ IDMAIN);
                string path = @"\\" + AppSettings.IPAddressFileServer + @"\BookAddInf\"+Fund.ToUpper()+@"\" + NewPath;
                

                DirectoryInfo di = new DirectoryInfo(path);
                if (!di.Exists)
                {
                    di.Create();
                }
                FileInfo[] ExistingCovers;

                ExistingCovers = di.GetFiles();

                List<string> MD5Hashes = new List<string>();

                foreach (FileInfo file in ExistingCovers)
                {
                    MD5Hashes.Add(Extensions.CalculateMD5(file.FullName));//собираем хэши существующих обложек
                }

                BJDatabaseWrapper wrapper = new BJDatabaseWrapper(Fund);
                DataTable pics = dbWrapper.GetImage(IDMAIN);
                foreach (DataRow pic in pics.Rows)//Копируем считанные из базы обложки на файловое хранилище
                {
                    byte[] p = (byte[])pic["PIC"];
                    Image img;
                    using (MemoryStream ms = new MemoryStream(p))
                    {
                        img = Image.FromStream(ms);
                    }
                    
                    StringBuilder fileName = new StringBuilder();
                    fileName.Append(path).Append("cover.").Append("jpg");//вроде в базе bj все обложки jpg...

                    if (!File.Exists(path + "cover.jpg"))//если нет файла с таким именем, значит нет обложки. сразу сохраняем
                    {
                        //img.Save(path + "cover.jpg");

                        using (FileStream fs = new FileStream(path + "cover.jpg", FileMode.Create, FileAccess.Write))
                        {
                            fs.Write(p, 0, p.Length);
                            fs.Flush();
                        }
                        if (img != null) img.Dispose();
                        continue;
                    }
                    else
                    {
                        //там уже могут лежать какие-то поэтому надо сравнивать MD5 хэши, чтобы дублей не запилить
                        string imgMD5 = Extensions.CalculateMD5(p);
                        if (MD5Hashes.Contains(imgMD5))//нашлась такая же обложка, значит её не сохраняем и пропускаем
                        {
                            if (img != null) img.Dispose();
                            continue;
                        }
                       
                        while (true)//если до сюда дошло, значит надо сохранить во что бы то ни стало
                        {
                            int i = 0;
                            if (!File.Exists(path + (i++).ToString() + ".jpg"))
                            {
                                string filename = path + (i).ToString() + ".jpg";
                                //img.Save(filename, ImageFormat.Jpeg);
                                using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(p, 0, p.Length);
                                    fs.Flush();
                                }
                                break;
                            }
                        }
                    }
                    if (img != null) img.Dispose();
                }
            }
        }
       
    

        private void AddHierarchyFields(int ParentPIN, int CurrentPIN, VufindDoc vfDoc)
        {
            DataTable table = dbWrapper.GetBJRecord(ParentPIN);
            int TopHierarchyId = GetTopId(ParentPIN);
            
            //AddField("hierarchy_top_id", this.Fund + "_" + TopHierarchyId);
            vfDoc.hierarchy_top_id.ValueList.Add(this.Fund + "_" + TopHierarchyId);

            table = dbWrapper.GetTitle(TopHierarchyId);
            if (table.Rows.Count != 0)
            {
                string hierarchy_top_title = table.Rows[0]["PLAIN"].ToString();
                vfDoc.hierarchy_top_title.ValueList.Add(hierarchy_top_title);
            }
            vfDoc.hierarchy_parent_id.ValueList.Add(this.Fund + "_" + ParentPIN);
            table = dbWrapper.GetTitle(ParentPIN);
            if (table.Rows.Count != 0)
            {
                string hierarchy_parent_title = table.Rows[0]["PLAIN"].ToString();
                vfDoc.hierarchy_parent_title.ValueList.Add(hierarchy_parent_title);
            }

            if (vfDoc.is_hierarchy_id.ValueList.Count == 0)
            {
                vfDoc.is_hierarchy_id.ValueList.Add(this.Fund + "_" + CurrentPIN);
            }

            table = dbWrapper.GetTitle(CurrentPIN);
            if (table.Rows.Count != 0)
            {
                string is_hierarchy_title = table.Rows[0]["PLAIN"].ToString();
                if (vfDoc.is_hierarchy_title.ValueList.Count == 0)
                {
                    vfDoc.is_hierarchy_title.ValueList.Add(is_hierarchy_title);
                }

            }
        }
        private int GetTopId(int ParentPIN)
        {
            DataTable table = dbWrapper.GetParentIDMAIN(ParentPIN);
            if (table.Rows.Count == 0)
            {
                return ParentPIN;
            }
            ParentPIN = Convert.ToInt32(table.Rows[0]["SORT"]);
            return GetTopId(ParentPIN);
        }
        private int GetMaxIDMAIN()
        {
            DataTable table = dbWrapper.GetMaxIDMAIN();
            return int.Parse(table.Rows[0][0].ToString());
        }
        private AuthoritativeFile GetAFAll( int AFLinkId, string AFTable)
        {
            //NAMES: (1) Персоналии
            //ORGS: (2) Организации
            //HEADER: (3) Унифицированное заглавие
            //DEL: (5) Источник списания
            //GEO: (6) Географическое название

            AuthoritativeFile result = new AuthoritativeFile();
            DataTable table = dbWrapper.GetAFAllValues(AFTable, AFLinkId);
            foreach (DataRow r in table.Rows)
            {
                result.Add(r["PLAIN"].ToString());
            }
            return result;
        }

    }
}
