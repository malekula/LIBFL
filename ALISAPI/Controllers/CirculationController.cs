﻿using LibflClassLibrary.ALISAPI.ResponseObjects.Books;
using LibflClassLibrary.Circulation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using LibflClassLibrary.ALISAPI.RequestObjects.Circulation;
using ALISAPI.Errors;

namespace ALISAPI.Controllers
{
    public class CirculationController : ApiController
    {
        
        /// <summary>
        /// Получает содержимое корзины читателя по номеру читательского билета
        /// </summary>
        /// <param name="idReader">Номер читательского билета</param>
        /// <returns>Содержимое корзины</returns>
        [HttpGet]
        [Route("Circulation/Basket/{idReader}")]
        [ResponseType(typeof(List<BookSimpleView>))]
        public HttpResponseMessage Get([Description("Номер чит билета")]int idReader)
        {

            CirculationInfo Circulation = new CirculationInfo();
            List<BookSimpleView> result = new List<BookSimpleView>();
            List<BasketInfo> basket;
            try
            {
                basket = Circulation.GetBasket(idReader);
            }
            catch (Exception ex)
            {
                return ALISErrorFactory.CreateError("G001", Request, HttpStatusCode.NotFound);
            }
            foreach (BasketInfo bi in basket)
            {
                result.Add(bi.Book);
            }

            return ALISResponseFactory.CreateResponse(result, Request);
        }

        /// <summary>
        /// Получает заказы читателя и их статусы
        /// </summary>
        /// <param name="idReader">Номер читательского билета</param>
        /// <returns>Заказы читателя</returns>
        [HttpGet]
        [Route("Circulation/Orders/{idReader}")]
        [ResponseType(typeof(List<BookSimpleView>))]
        public HttpResponseMessage Orders([Description("Номер чит билета")]int idReader)
        {

            CirculationInfo Circulation = new CirculationInfo();
            List<OrderInfo> result = new List<OrderInfo>();
            List<BasketInfo> basket;
            try
            {
                result = Circulation.GetOrders(idReader);
            }
            catch (Exception ex)
            {
                return ALISErrorFactory.CreateError("G001", Request, HttpStatusCode.NotFound);
            }
            //foreach (BasketInfo bi in basket)
            //{
            //    result.Add(bi.Book);
            //}

            return ALISResponseFactory.CreateResponse(result, Request);
        }


        /// <summary>
        /// Вставить в персональную корзину читателя список id книг. Метод нужно вызывать после авторизации.
        /// </summary>
        /// <returns>HTTP200</returns>
        [HttpPost]
        [Route("Circulation/InsertIntoUserBasket")]
        //[ResponseType(typeof(ReaderInfo))]
        public HttpResponseMessage InsertIntoUserBasket()
        {
            string JSONRequest = Request.Content.ReadAsStringAsync().Result;
            ImpersonalBasket request;
            try
            {
                request = JsonConvert.DeserializeObject<ImpersonalBasket>(JSONRequest, ALISSettings.ALISDateFormatJSONSettings);
            }
            catch
            {
                return ALISErrorFactory.CreateError("G001", Request, HttpStatusCode.BadRequest);
            }

            CirculationInfo Circulation = new CirculationInfo();


            try
            {
                Circulation.InsertIntoUserBasket(request);
            }
            catch (Exception ex)
            {
                return ALISErrorFactory.CreateError(ex.Message, Request, HttpStatusCode.InternalServerError);
            }
            return ALISResponseFactory.CreateResponse(Request);
        }






















        //// GET: api/Circulation
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET: api/Circulation/5
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST: api/Circulation
        //public void Post([FromBody]string value)
        //{
        //}

        //// PUT: api/Circulation/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE: api/Circulation/5
        //public void Delete(int id)
        //{
        //}
    }
}