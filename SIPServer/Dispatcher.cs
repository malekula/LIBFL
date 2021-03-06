﻿using ALISAPI.Errors;
using LibflClassLibrary.ALISAPI.Errors;
using LibflClassLibrary.BJUsers;
using LibflClassLibrary.Books;
using LibflClassLibrary.Books.BJBooks;
using LibflClassLibrary.Books.BJBooks.BJExemplars;
using LibflClassLibrary.Circulation;
using LibflClassLibrary.ExportToVufind;
using LibflClassLibrary.Readers;
using LibflClassLibrary.SipServer;
using SipLibrary.Abstract;
using SipLibrary.Messages.Requests;
using SipLibrary.Messages.Responses;
using SipLibrary.Transport;
using SipLibrary.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LibflClassLibrary.Circulation.CirculationInfo;
using NLog;

namespace SIPServer
{
    public class Dispatcher : IDispatcher
    {

        List<SipClientInfo> clients_ = new List<SipClientInfo>();
        SipServerHandler handler_ = new SipServerHandler();
        private static Logger logger = LogManager.GetCurrentClassLogger();


        public void OnConnected(Session session)
        {
            SipClientInfo client = new SipClientInfo();
            client.session = session;
            client = null;
            client = clients_.Find(x => x.session.Ip == session.Ip);
            if (client == null)
            {
                client = new SipClientInfo();
                client.session = session;
                clients_.Add(client);
                Console.WriteLine("New Client Connected");
            }
            else
            {
                Console.WriteLine("Client Has Already Connected");
            }
        }

        public void OnDisconnected(Session session)
        {
            SipClientInfo client = clients_.Find(x => x.session.Ip == session.Ip);
            if (client == null)
            {
                Console.WriteLine("Client was not found in connected clients list.");
            }
            else
            {
                clients_.Remove(client);
                Console.WriteLine("Client Disconnected");
            }
        }

        public void OnLogin(Session session, LoginRequest request, LoginResponse response)
        {

            if (!CheckSCLoginStatus(session))
            {
                response.Ok = false;
                Console.WriteLine("Client was not connected. Login impossible");
                return;
            }
            else
            {
                SipClientInfo client = clients_.Find(x => x.session.Ip == session.Ip);
                BJUserInfo bjUser = BJUserInfo.GetUserByLogin(request.LoginUserId, "BJVVV");
                if (bjUser == null)
                {
                    Console.WriteLine("Login failed. Username or password is invalid.");
                    response.Ok = false;
                }
                else
                {
                    if (BJUserInfo.HashPassword(request.LoginPassword) == bjUser.HashedPwd)
                    {
                        client.login = bjUser.Login;
                        client.bjUser = bjUser;
                        client.locationCode = "Hall 2nd floor";
                        client.bjUser.SelectedUserStatus = bjUser.UserStatus[0];
                        response.Ok = true;
                        Console.WriteLine("Successful login.");
                        return;
                    }
                }
            }

            //SipClientInfo client = new SipClientInfo();
            //client.login = "station2";
            //client.bjUser = new BJUserInfo();
            //client.locationCode = "Hall 2nd floor";
            //UserStatus us = new UserStatus();
            //us.DepId = 2033;
            //us.DepName = "Hall 2nd floor";
            //client.bjUser.S
        }


        public void OnPatronInformation(Session session, PatronInformationRequest request, PatronInformationResponse response)
        {

            Console.WriteLine("PatronInformation Message");

            ReaderInfo reader = handler_.GetPatron(request.PatronId);
            if (reader == null)
            {
                response.PartonStatus = new PatronStatus();
                response.Language = Language.Russian;
                response.TransactionDate = DateTime.Now;
                response.HoldItemsCount = 0;
                response.OverdueItemsCount = 0;
                response.ChargedItemsCount = 0;
                response.RecallItemsCount = 0;
                response.FineItemsCount = 0;
                response.UnavailableHoldsCount = 0;
                response.InstitutionId = request.InstitutionId;
                response.PatronIdentifier = request.PatronId;
                response.PersonalName = "Читатель не найден";
                response.ValidPatron = false;
                Console.WriteLine("Читатель не найден");
                return;
            }
            else
            {
                response.PartonStatus = new PatronStatus();
                response.Language = Language.Russian;
                response.TransactionDate = DateTime.Now;

                CirculationInfo ci = new CirculationInfo();
                List<OrderInfo> orders = ci.GetOrders(reader.NumberReader);
                List<OrderInfo> chargedOrders = orders.FindAll(x => x.StatusCode == CirculationStatuses.IssuedAtHome.Id || x.StatusCode == CirculationStatuses.IssuedInHall.Id);
                List<OrderInfo> holdOrders = orders.FindAll(x => x.StatusCode == CirculationStatuses.EmployeeLookingForBook.Id || x.StatusCode == CirculationStatuses.OrderIsFormed.Id
                                                            || x.StatusCode == CirculationStatuses.WaitingFirstIssue.Id || x.StatusCode == CirculationStatuses.InReserve.Id);
                List<OrderInfo> overdueOrders = chargedOrders.FindAll(x => x.ReturnDate < DateTime.Now);
                response.HoldItemsCount = holdOrders.Count;
                response.OverdueItemsCount = overdueOrders.Count;
                response.ChargedItemsCount = chargedOrders.Count;
                response.FineItemsCount = 0;
                response.RecallItemsCount = 0;
                response.UnavailableHoldsCount = 0;
                response.InstitutionId = request.InstitutionId;
                response.PatronIdentifier = request.PatronId;
                response.PersonalName = $"{reader.FamilyName} {reader.Name} {reader.FatherName}";

                response.HoldItemsLimit = 100;
                response.OverdueItemsLimit = 667;
                response.ChargedItemsLimit = 100;

                response.ValidPatron = true;
                response.ValidPatronPassword = string.Empty;

                response.CurrencyType = Currency.RUB;

                response.FeeAmount = 0;
                response.FeeLimit = 1000;

                response.HomeAddress = reader.RegistrationCity;
                response.EmailAddress = reader.Email;
                response.HomePhoneNumber = reader.MobileTelephone;

                response.ScreenMessage = "Информация о читателе";
                response.PrintLine = "";

                foreach (var order in chargedOrders)
                {
                    ExemplarBase exemplar = ExemplarFactory.CreateExemplar(order.ExemplarId, order.Fund);
                    if (exemplar == null)
                    {
                        BJExemplarInfo fake = new BJExemplarInfo();
                        fake.Bar = $"Экземпляр удалён из базы. Id = {order.ExemplarId}, Fund = {order.Fund}";
                        fake.Title = $"Экземпляр удалён из базы. Id = {order.ExemplarId}, Fund = {order.Fund}";
                        exemplar = fake;
                    }
                    response.ChargedItems.Add(exemplar.Bar);
                }
                foreach (var order in holdOrders)
                {
                    ExemplarBase exemplar = ExemplarFactory.CreateExemplar(order.ExemplarId, order.Fund);
                    if (exemplar == null)
                    {
                        BJExemplarInfo fake = new BJExemplarInfo();
                        fake.Bar = $"Экземпляр удалён из базы. Id = {order.ExemplarId}, Fund = {order.Fund}";
                        fake.Title = $"Экземпляр удалён из базы. Id = {order.ExemplarId}, Fund = {order.Fund}";
                        exemplar = fake;
                    }
                    if (exemplar.Bar != null) response.HoldItems.Add(exemplar.Bar);
                    else if (exemplar.InventoryNumber != null) response.HoldItems.Add(exemplar.InventoryNumber);
                    else response.HoldItems.Add(exemplar.Id);
                }
            }
        }

        public void OnScStatus(Session session, ScStatusRequest request, AcsStatusResponse response)
        {
            Console.WriteLine("Sc Status Message");
            //request.
            SipClientInfo client = clients_.Find(x => x.session.Ip == session.Ip);
            response.OnlineStatus = (client == null) ? false : true;
            response.CheckInOk = true;
            response.CheckOutOk = true;
            response.RenewalPolicy = true;
            response.StatusUpdateOk = true;
            response.OfflineOk = false;
            response.TimeOutPeriod = 100;
            response.RetiresAllowed = 3;
            response.InstitutionId = "ВГБИЛ";
            response.SupportedMessages = SupportedMessages.DefaultValue();
            response.SupportedMessages.PatronInformation = true;
            response.ProtocolVersion = ProtocolVersion.VERSION_2_00;
            response.TerminalLocation = (client == null) ? $"Станция не залогинена. IP = {session.Ip}" : client.bjUser.Login;
            response.DateTimeSync = DateTime.Now;
        }

        public void OnItemInformation(Session session, ItemInformationRequest request, ItemInformationResponse response)
        {
            Console.WriteLine("Item Information Message");

            string bar = request.ItemIdentifier;


            BookBase book;
            ExemplarBase exemplar;
            CirculationInfo ci = new CirculationInfo();
            OrderInfo order;
            try
            {
                exemplar = ExemplarFactory.CreateExemplar(bar);
                book = BookFactory.CreateBookByPin(exemplar.BookId);
                order = ci.FindOrderByExemplar(exemplar);
            }
            catch (Exception ex)
            {
                response.CirculationStatus = CirculationStatus.MISSING;
                response.SecurityMarker = SecurityMarker.OTHER;
                response.FeeType = FeeType.ADMINISTRATIVE;
                response.TransactionDate = DateTime.Now;
                response.ItemIdentifier = request.ItemIdentifier;
                exemplar = ExemplarFactory.CreateExemplar(bar);
                response.ScreenMessage = "Книга не найдена!";
                //logger.Error($"{DateTime.Now.ToString()} mation. \n" + ex.Message);
                return;
            }

            if (order == null)
            {
                response.CirculationStatus = CirculationStatus.AVAILABLE;
            }
            else
            {
                switch (order.StatusName)
                {
                    case CirculationStatuses.ElectronicIssue.Value:
                        response.CirculationStatus = CirculationStatus.AVAILABLE;
                        break;
                    case CirculationStatuses.EmployeeLookingForBook.Value:
                        response.CirculationStatus = CirculationStatus.AVAILABLE;
                        break;
                    case CirculationStatuses.Finished.Value:
                        response.CirculationStatus = CirculationStatus.AVAILABLE;
                        break;
                    case CirculationStatuses.ForReturnToBookStorage.Value:
                        response.CirculationStatus = CirculationStatus.AVAILABLE;
                        break;
                    case CirculationStatuses.InReserve.Value:
                        response.CirculationStatus = CirculationStatus.AVAILABLE;
                        break;
                    case CirculationStatuses.IssuedAtHome.Value:
                        response.CirculationStatus = CirculationStatus.CHARGED;
                        break;
                    case CirculationStatuses.IssuedFromAnotherReserve.Value:
                        response.CirculationStatus = CirculationStatus.CHARGED;
                        break;
                    case CirculationStatuses.IssuedInHall.Value:
                        response.CirculationStatus = CirculationStatus.CHARGED;
                        break;
                    case CirculationStatuses.OrderIsFormed.Value:
                        response.CirculationStatus = CirculationStatus.AVAILABLE;
                        break;
                    case CirculationStatuses.Prolonged.Value:
                        response.CirculationStatus = CirculationStatus.AVAILABLE;
                        break;
                    case CirculationStatuses.SelfOrder.Value:
                        response.CirculationStatus = CirculationStatus.AVAILABLE;
                        break;
                    case CirculationStatuses.WaitingFirstIssue.Value:
                        response.CirculationStatus = CirculationStatus.AVAILABLE;
                        break;
                }
            }

            //здесь вообще-то SecurityMarker - это что-то другое. мы в сотрудничестве с IDLogic даём тут этому параметру другое предназначение
            //это поле будет сигнализировать о том, снимать или нет противокражный бит. 
            //SecurityMarker.OTHER - "00" - не будет противокражка сниматься. это для книг, которые только в зал
            //SecurityMarker.NONE - "01" - будет противокражка сниматься. Для книг, которые на дом
            //таким образом узнаем способ доступа для конкретного экземпляра и присвоим соответствующее значение.
            //response.SecurityMarker = SecurityMarker.NONE;
            IssueType it = ci.GetIssueType(exemplar);
            if (it == IssueType.AtHome)
            {
                response.SecurityMarker = SecurityMarker.NONE;
            }
            else
            {
                response.SecurityMarker = SecurityMarker.OTHER;
            }
            response.FeeType = FeeType.ADMINISTRATIVE;
            DateTime dateReaderNeedToReturn;
            if (order != null)
            {
                dateReaderNeedToReturn = (it == IssueType.AtHome) ? order.ReturnDate : DateTime.Now;
            }
            else
            {
                dateReaderNeedToReturn = (it == IssueType.AtHome) ? DateTime.Now.AddDays(30) : DateTime.Now;
            }


            response.TransactionDate = DateTime.Now;
            response.HoldQueueLength = 0;
            response.DueDate = dateReaderNeedToReturn;//(order == null) ? DateTime.Now : order.ReturnDate;
            response.RecallDate = dateReaderNeedToReturn;// (order == null) ? DateTime.Now : order.IssueDate;
            response.HoldPickupDate = dateReaderNeedToReturn;//(order == null) ? DateTime.Now : order.ReturnDate;
            response.ItemIdentifier = request.ItemIdentifier;
            response.TitleIdentifier = book.AuthorTitle;
            response.Owner = "ВГБИЛ";
            response.CurrencyType = Currency.RUB;
            response.FeeAmount = 0;
            response.MediaType = MediaType.BOOK;
            response.PermanentLocation = KeyValueMapping.UnifiedLocationAccess[exemplar.Location];
            response.CurrentLocation = KeyValueMapping.UnifiedLocationAccess[exemplar.Location];
            response.ItemProperties = "";//string.Empty;
            response.ScreenMessage = ""; //string.Empty;// 
            response.PrintLine = ""; //string.Empty;// 
        }

        public void OnCheckout(Session session, CheckoutRequest request, CheckoutResponse response)
        {
            Console.WriteLine("Checkout Message");
            SipClientInfo client = clients_.Find(x => x.session.Ip == session.Ip);
            //if (client == null)
            //{
            //    FillCheckoutFailedResponse(response, request, "");
            //    Console.WriteLine("Client is not logged in. CheckOut operation impossible.");
            //    return;
            //}
            //if (client.bjUser == null)
            //{
            //    FillCheckoutFailedResponse(response, request, "");
            //    Console.WriteLine("NOT LOGGED IN");
            //    return;
            //}

            
            ExemplarBase exemplar;
            CirculationInfo ci;
            OrderInfo order;
            BookBase book;
            ReaderInfo reader;
            try
            {
                exemplar = ExemplarFactory.CreateExemplar(request.ItemIdentifier);
                ci = new CirculationInfo();
                order = ci.FindOrderByExemplar(exemplar);
                book = BookFactory.CreateBookByPin(exemplar.BookId);
                reader = handler_.GetPatron(request.PatronIdentifier);
            }
            catch (Exception ex)
            {
                Console.WriteLine("320" + ex.Message + ex.Source + ex.Data + ex.StackTrace);
                logger.Error($"{DateTime.Now.ToString()} Ошибка в OnCheckout. \n" + ex.Message);
                FillCheckoutFailedResponse(response, request, ex.Message);
                return;
            }
            if (reader == null)
            {
                Console.WriteLine("Читатель не найден");
                //logger.Error($"{DateTime.Now.ToString()} Ошибка в OnCheckout. \n" + ex.Message);
                FillCheckoutFailedResponse(response, request, $"Выдача невозможна! Читатель с идентификатором {request.PatronIdentifier} не найден!");
                return;
            }

            // обязательные поля
            response.Ok = true;
            if (order != null)
            {
                response.RenewalOk = (order.ReaderId.ToString() == request.PatronIdentifier) &&
                                     ((order.StatusName == CirculationStatuses.IssuedAtHome.Value) || (order.StatusName == CirculationStatuses.IssuedInHall.Value))
                                     ? true : false;
            }
            else
            {
                response.RenewalOk = false;
            }
            if (order == null)
            {
                try
                {
                    ci.IssueBookToReader(exemplar, reader, client.bjUser);
                    order = ci.FindOrderByExemplar(exemplar);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("377" + ex.Message + ex.Source + ex.Data + ex.StackTrace);
                    FillCheckoutFailedResponse(response, request, ex.Message);
                    logger.Error($"{DateTime.Now.ToString()} Ошибка в OnCheckout. \n" + ex.Message);
                    return;
                }
            }
            else
            {
                try
                {
                    ci.ChangeOrderStatus(client.bjUser, order.OrderId, CirculationStatuses.Finished.Value);
                    ci.IssueBookToReader(exemplar, reader, client.bjUser);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("391" + ex.Message + ex.Source + ex.Data + ex.StackTrace);
                    logger.Error($"{DateTime.Now.ToString()} Ошибка в OnCheckout. \n" + ex.Message);
                    FillCheckoutFailedResponse(response, request, ex.Message);
                    return;
                }
            }

            //Desentize - раньше я думал, что это отвечает за снятие противокражкин, но оказалось, что нет
            //правильно я раньше думал!
            IssueType issueType = ci.GetIssueType(exemplar);
            response.Desensitize = (issueType == IssueType.AtHome) ? true : false; //снять бит или нет

            response.MagneticMedia = false;
            response.TransactionDate = DateTime.Now;
            response.InstitutionId = "ВГБИЛ";
            response.PatronIdentifier = request.PatronIdentifier;// + " (took from request)";
            response.ItemIdentifier = request.ItemIdentifier;// + " (also took from request)";
            response.TitleIdentifier = book.AuthorTitle;
            order = ci.FindOrderByExemplar(exemplar);
            if (order != null)
            {
                if (issueType == IssueType.InHall)
                {
                    response.DueDate = DateTime.Now;
                }
                else
                {
                    response.DueDate = order.ReturnDate;
                }
            }
            else
            {
                response.DueDate = DateTime.Now;
            }

            // необязательные поля
            response.FeeType = FeeType.OTHER_UNKNOWN;
            response.SecurityInhibit = true;
            response.CurrencyType = Currency.RUB;
            response.FeeAmount = 0;
            response.MediaType = MediaType.BOOK;
            response.ItemProperties = string.Empty;//"The SUPA DUPA Properies";
            response.TransactionId = order.OrderId.ToString();//"The Trans Id";

            response.ScreenMessage = string.Empty;// "F**K The Police";
            response.PrintLine = string.Empty;// "The Print Line";
        }

        private void FillCheckoutFailedResponse(CheckoutResponse response, CheckoutRequest request, string message)
        {
            response.Ok = false;
            response.RenewalOk = false;
            response.MagneticMedia = false;
            response.Desensitize = false;
            response.TransactionDate = DateTime.Now;
            response.InstitutionId = request.InstitutionId;
            response.PatronIdentifier = request.PatronIdentifier;
            response.ItemIdentifier = request.ItemIdentifier;
            ExemplarBase exemplar = ExemplarFactory.CreateExemplar(request.ItemIdentifier);

            BookBase book = (exemplar == null) ? null : BookFactory.CreateBookByPin(exemplar.BookId);
            var ci = new CirculationInfo();
            var order = (exemplar == null) ? null : ci.FindOrderByExemplar(exemplar);

            response.TitleIdentifier = (book == null) ? "Неизвестная книга" : book.AuthorTitle;
            IssueType issueType = ci.GetIssueType(exemplar);

            if (order != null)
            {
                if (issueType == IssueType.InHall)
                {
                    response.DueDate = DateTime.Now;
                }
                else
                {
                    response.DueDate = order.ReturnDate;
                }
            }
            else
            {
                response.DueDate = DateTime.Now;
            }

            if (!string.IsNullOrEmpty(message))
            {
                ALISError error = ALISErrorList._list.Find(x => x.Code == message);
                response.ScreenMessage = (error != null) ? error.Message : message;           
            }
            else
            {
                response.ScreenMessage = "Неизвестная ошибка";
            }
        }
        private void FillCheckinFailedResponse(CheckinResponse response, CheckinRequest request)
        {

            //поправить этот метод.
            response.Ok = false;
            response.Resensitize = true;
            response.MagneticMedia = false;
            response.Alert = false;
            response.TransactionDate = DateTime.Now;
            response.InstitutionId = request.InstitutionId;
            response.ItemIdentifier = request.ItemIdentifier;
            ExemplarBase exemplar = ExemplarFactory.CreateExemplar(request.ItemIdentifier);
            BookBase book = (exemplar == null) ? null : BookFactory.CreateBookByPin(exemplar.BookId);

            response.PermanentLocation = (exemplar == null) ? "Неизвестно" : exemplar.Location;
            response.TitleIdentifier = (book == null) ? "Неизвестная книга" : book.AuthorTitle ;
        }
        public void OnCheckin(Session session, CheckinRequest request, CheckinResponse response)
        {
            Console.WriteLine("Checkin Message");

            SipClientInfo client = clients_.Find(x => x.session.Ip == session.Ip);
            if (client == null)
            {
                FillCheckinFailedResponse(response, request);
                Console.WriteLine("Client is not logged in. CheckOut operation impossible.");
                return;
            }



            ExemplarBase exemplar;
            CirculationInfo ci;
            OrderInfo order;
            BookBase book;
            ReaderInfo reader;
            try
            {
                exemplar = ExemplarFactory.CreateExemplar(request.ItemIdentifier);
                ci = new CirculationInfo();
                order = ci.FindOrderByExemplar(exemplar);
                if (order == null)
                {
                    //никому не выдана
                    FillCheckinFailedResponse(response, request);
                    return;
                }
                if (order.StatusName == CirculationStatuses.Finished.Value || order.StatusName == CirculationStatuses.Refusual.Value || order.StatusName == CirculationStatuses.SelfOrder.Value)
                {
                    //никому не выдана
                    FillCheckinFailedResponse(response, request);
                    return;
                }
                book = BookFactory.CreateBookByPin(exemplar.BookId);
                reader = ReaderInfo.GetReader(order.ReaderId);
            }
            catch (Exception ex)
            {
                FillCheckinFailedResponse(response, request);
                Console.WriteLine(ex.Message);
                response.Ok = false;
                logger.Error($"{DateTime.Now.ToString()} Ошибка в OnCheckin. \n" + ex.Message);
                return;
            }

            try
            {
                exemplar.circulation.exemplarRecieverFromReader.RecieveBookFromReader(exemplar, order, client.bjUser);
                //ci.RecieveBookFromReader(exemplar, order, client.bjUser);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                FillCheckinFailedResponse(response, request);
                logger.Error($"{DateTime.Now.ToString()} Ошибка в OnCheckin. \n" + ex.Message);
                return;
            }


            response.Ok = true;
            response.Resensitize = true;
            response.MagneticMedia = null;
            response.Alert = false;
            response.TransactionDate = DateTime.Now;
            response.InstitutionId = "ВГБИЛ";
            response.ItemIdentifier = request.ItemIdentifier;
            response.PermanentLocation = KeyValueMapping.UnifiedLocationAccess[exemplar.Location];//"Permanent Location";
            response.TitleIdentifier = book.AuthorTitle;
            response.SortBin = "Sort bin...";
            response.PatronIdentifier = reader.BarCode;//"Patron id";
            response.MediaType = MediaType.BOOK;
            response.ItemProperties = "Propst";

            response.MagneticMedia = false;

            response.ScreenMessage = "Книга успешно принята";
            response.PrintLine = "";
        }
        private void FillRenewFailedResponse(RenewResponse response, RenewRequest request)
        {
            response.Ok = false;
            response.RenewalOk = false;
            response.MagneticMedia = false;
            response.Desensitize = false;
            response.TransactionDate = DateTime.Now;
            response.InstitutionId = request.InstitutionId;
            response.PatronIdentifier = request.PatronIdentifier;
            response.ItemIdentifier = request.ItemIdentifier;
            CirculationInfo ci = new CirculationInfo();
            ExemplarBase exemplar = ExemplarFactory.CreateExemplar(request.ItemIdentifier);
            if (exemplar != null)
            {
                OrderInfo order = ci.GetLastOrder(Convert.ToInt32(exemplar.Id), exemplar.Fund);
                response.DueDate = order.ReturnDate;
            }
            else
            {
                response.DueDate = DateTime.Now;

            }
            response.TitleIdentifier = exemplar.AuthorTitle;

        }
        public void OnRenew(Session session, RenewRequest request, RenewResponse response)
        {
            Console.WriteLine("Renew Message");
            SipClientInfo client = clients_.Find(x => x.session.Ip == session.Ip);
            if (client == null)
            {
                FillRenewFailedResponse(response, request);
                Console.WriteLine("Client is not logged in. CheckOut operation impossible.");
                return;
            }

            bool result = false;
            CirculationInfo ci = new CirculationInfo();
            ExemplarBase exemplar;
            OrderInfo order = new OrderInfo();
            BookBase book;
            try
            {
                exemplar = ExemplarFactory.CreateExemplar(request.ItemIdentifier);
                order = ci.FindOrderByExemplar(exemplar);
                book = BookFactory.CreateBookByPin(exemplar.BookId);
            }
            catch (Exception ex)
            {
                ALISError error = ALISErrorList._list.Find(x => x.Code == ex.Message);
                response.ScreenMessage = (error != null) ? error.Message : ex.Message;
                FillRenewFailedResponse(response, request);
                logger.Error($"{DateTime.Now.ToString()} Ошибка в OnRenew. \n" + ex.Message);
                return;
            }

            if (order == null)
            {
                FillRenewFailedResponse(response, request);
            }
            else
            {
                response.RenewalOk = true;
            }

            if (order != null)
            {
                try
                {
                    ci.ProlongOrder(order.OrderId);
                    order = ci.FindOrderByExemplar(exemplar);
                    response.RenewalOk = true;
                    result = true;
                }
                catch (Exception ex)
                {
                    result = false;
                    ALISError error = ALISErrorList._list.Find(x => x.Code == ex.Message);
                    response.ScreenMessage = (error != null) ? error.Message : ex.Message;
                    logger.Error($"{DateTime.Now.ToString()} Ошибка в OnRenew. \n" + ex.Message);
                }
            }

            order = ci.FindOrderByExemplar(exemplar);


            response.Ok = result;
            response.Desensitize = false;


            response.MagneticMedia = ThreeStateBool.False;

            response.TransactionDate = DateTime.Now;

            response.InstitutionId = "ВГБИЛ";
            response.PatronIdentifier = request.PatronIdentifier;
            response.ItemIdentifier = request.ItemIdentifier;
            response.TitleIdentifier = book.AuthorTitle;

            IssueType it = ci.GetIssueType(exemplar);
            DateTime dateReaderNeedToReturn;
            if (order != null)
            {
                dateReaderNeedToReturn = (it == IssueType.AtHome) ? order.ReturnDate : DateTime.Now;
            }
            else
            {
                dateReaderNeedToReturn = (it == IssueType.AtHome) ? DateTime.Now.AddDays(30) : DateTime.Now;
            }

            response.DueDate = dateReaderNeedToReturn;

            response.FeeType = FeeType.OTHER_UNKNOWN;
            response.SecurityInhibit = false;
            response.CurrencyType = Currency.RUB;
            response.FeeAmount = 0;
            response.MediaType = MediaType.BOOK;

            response.ItemProperties = "";
            response.TransactionId = order.OrderId.ToString();
            if (result == false)
            {
                response.ScreenMessage = "";
            }
            response.PrintLine = (response.Ok == true) ? "Продлено успешно" : "Не продлено";
        }

        public void OnRenewAll(Session session, RenewAllRequest request, RenewAllResponse response)
        {
            Console.WriteLine("RenewAll Message");

            response.Ok = true;
            response.RenewedCount = 3;
            response.UnrenewedCount = 1;
            response.TransactionDate = DateTime.Now;
            response.InstitutionId = "Inst Id";

            response.RenewedItems.Add("The Renewed Item #1");
            response.RenewedItems.Add("The Renewed Item #2");
            response.RenewedItems.Add("The Renewed Item #2");

            response.UnrenewedItems.Add("The Failed Item #1");

            response.ScreenMessage = "Screen Message";
            response.PrintLine = "Printline #1";
        }

        public void OnEndPatronSession(Session session, EndPatronSessionRequest request, EndPatronSessionResponse response)
        {
            Console.WriteLine("EndPatronSession Message");

            response.EndSession = true;
            response.TransactionDate = DateTime.Now;

            response.InstitutionId = "The Institiotion Id";
            response.PatronIdentifier = request.PatronIdentifier + " (took from request)";

            response.ScreenMessage = "Screen Message";
            response.PrintLine = "Printline #1";
        }


        private bool CheckSCLoginStatus(Session session)
        {
            SipClientInfo client = clients_.Find(x => x.session.Ip == session.Ip);
            return (client == null) ? false : true;
        }


    }
}
