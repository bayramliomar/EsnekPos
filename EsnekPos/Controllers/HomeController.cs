using EsnekPos.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;
using static EsnekPos.Models.PaymentRequest;

namespace EsnekPos.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Step1()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Step1(EYVCreditCard creditCard, EYVConfig config, Result result)
        {
            try
            {
                using (var http = new HttpClient())
                {
                    PaymentRequest paymentRequest = new PaymentRequest();
                    paymentRequest.Config = new PaymentRequest.EYVConfig();
                    paymentRequest.Config.MERCHANT = "";
                    paymentRequest.Config.MERCHANT_KEY = "";
                    paymentRequest.Config.ORDER_AMOUNT = config.ORDER_AMOUNT;
                    paymentRequest.Config.PRICES_CURRENCY = "TRY";
                    paymentRequest.Config.BACK_URL = "http://localhost:58850/Home/Step2";
                    paymentRequest.Config.ORDER_REF_NUMBER = "";

                    paymentRequest.CreditCard = new PaymentRequest.EYVCreditCard();
                    paymentRequest.CreditCard.CC_OWNER = creditCard.CC_OWNER;
                    paymentRequest.CreditCard.CC_NUMBER = creditCard.CC_NUMBER.Replace(" ","");
                    paymentRequest.CreditCard.EXP_MONTH = creditCard.EXP_MONTH;
                    paymentRequest.CreditCard.EXP_YEAR = creditCard.EXP_YEAR;
                    paymentRequest.CreditCard.CC_CVV = creditCard.CC_CVV;
                    paymentRequest.CreditCard.INSTALLMENT_NUMBER = "0";

                    paymentRequest.Customer = new PaymentRequest.EYVCustomer();
                    paymentRequest.Customer.FIRST_NAME = "Müşteri Ad";
                    paymentRequest.Customer.LAST_NAME = "Müşteri Soyad";
                    paymentRequest.Customer.ADDRESS = "Müşteri Adres";
                    paymentRequest.Customer.MAIL = "Müşteri Mail Adresi";
                    paymentRequest.Customer.PHONE = "müşteri Telefon";
                    paymentRequest.Customer.CITY = "Şehir";
                    paymentRequest.Customer.STATE = "İlçe";

                    var content = new StringContent(JsonConvert.SerializeObject(paymentRequest));
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var request = http.PostAsync("link", content);

                    var response = request.Result.Content.ReadAsStringAsync().Result;
                    var result_ = JsonConvert.DeserializeObject<Result>(response);
                    if (result_.STATUS == "SUCCESS" && result.RETURN_CODE=="0")
                    {
                        var client = new RestClient(result_.URL_3DS);
                        client.Timeout = -1;
                        var request_secure3dUrl = new RestRequest(Method.GET);
                        IRestResponse response_secure3dUrl = client.Execute(request_secure3dUrl);
                        TempData["list"] = response_secure3dUrl.Content;
                    }
                    return new JsonResult()
                    {
                        Data = JsonConvert.DeserializeObject<Result>(response),
                        MaxJsonLength = int.MaxValue,
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
            }
            catch (Exception ex)
            {
                return View();
            }
            
        }

        [HttpPost]
        public ActionResult Step2(Result model)
        {
            if(model.STATUS=="SUCCESS" && model.RETURN_CODE == "0")
            {
                return View();
            }
            return View();
        }
    }
}