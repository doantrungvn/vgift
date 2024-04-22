using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Web;
using VTCGame.Portal.Models;
using VTCGame.Utility;

namespace VTCGame.Portal
{
    public class LoginConfig
    {
        public static string Vcoinserviceid = ConfigurationManager.AppSettings["TopupVcoin_ByVcoinCard_ServiceID"];//serviceId
        public static string Vcoinservicekey = ConfigurationManager.AppSettings["TopupVcoin_ByVcoinCard_ServiceKey"];//serviceKey 
        public static string Vtserviceid = ConfigurationManager.AppSettings["TopupVcoin_ByVtelCard_ServiceID"];//serviceId
        public static string Vtservicekey = ConfigurationManager.AppSettings["TopupVcoin_ByVtelCard_ServiceKey"];//serviceKey
        public static string Mbserviceid = ConfigurationManager.AppSettings["TopupVcoin_ByMobiCard_ServiceID"];//serviceId
        public static string Mbservicekey = ConfigurationManager.AppSettings["TopupVcoin_ByMobiCard_ServiceKey"];//serviceKey
        public static string Vinaserviceid = ConfigurationManager.AppSettings["TopupVcoin_ByVinaCard_ServiceID"];//serviceId
        public static string Vinaservicekey = ConfigurationManager.AppSettings["TopupVcoin_ByVinaCard_ServiceKey"];//serviceKey
        public static string Vnmserviceid = ConfigurationManager.AppSettings["TopupVcoin_ByVNMCard_ServiceID"];//serviceId
        public static string Vnmservicekey = ConfigurationManager.AppSettings["TopupVcoin_ByVNMCard_ServiceKey"];//serviceKey

        public static int AccountId
        {
            get
            {
                try
                {
                    //lưu info vào cookie dạng accountId|userName|tokenKey|ServiceId_Old|billingAccessToken
                    if (HttpContext.Current.User.Identity.IsAuthenticated && HttpContext.Current.Request.ServerVariables["AUTH_USER"].ToString().Length > 0) //đã set form
                    {
                        var current = HttpContext.Current.Request.ServerVariables["AUTH_USER"].ToString();
                        //NLogLogger.LogDebug("Session Login" + current);
                        var arrCurrent = current.ToString().Split('|');
                        if (arrCurrent.Length >= 4)
                        {
                            var accountID = Convert.ToInt32(arrCurrent[0]);
                            if (accountID > 0) //&& ktra cookie . 
                            {
                                //kiem tra   is_trackking
                                try
                                {
                                    if (HttpContext.Current.Session["isTrackingNru"] == null)
                                    { //neu chua tracking  thi tracking 
                                        //Libs.Tracking.TrackingMeasure.TrackingNRU(106, accountID, HttpContext.Current.Session["LinkGen"], AccountName);//106 login 
                                        if (HttpContext.Current.Session["LinkGen"] != null)
                                        {
                                            TrackingApi.InsertNru(accountID, AccountName, 106, 9, HttpContext.Current.Session["LinkGen"].ToString());
                                        }
                                        //va set   tracking 
                                        HttpContext.Current.Session["isTrackingNru"] = "1";
                                    }
                                }
                                catch
                                {

                                }

                            }

                            return accountID;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    else
                    {
                        return 0;
                    }
                }
                catch (Exception ex)
                {
                    NLogLogger.LogInfo(ex.Message);
                    return -1;
                }
            }
        }
        public static string AccountName
        {
            get
            {
                try
                {
                    //lưu info vào cookie dạng accountId|userName|tokenKey|ServiceId_Old|billingAccessToken
                    if (HttpContext.Current.User.Identity.IsAuthenticated && HttpContext.Current.Request.ServerVariables["AUTH_USER"].ToString().Length > 0) //đã set form
                    {
                        var current = HttpContext.Current.Request.ServerVariables["AUTH_USER"].ToString();
                        var arrCurrent = current.ToString().Split('|');
                        if (arrCurrent.Length >= 4)
                        {
                            var accountName = arrCurrent[1];
                            return accountName;
                        }
                        else
                        {
                            return string.Empty;
                        }
                    }
                    else
                    {
                        return string.Empty;
                    }


                }
                catch (Exception ex)
                {
                    NLogLogger.LogInfo(ex.Message);
                    return string.Empty;
                }
            }
        }
        public static string AccessToken
        {
            get
            {
                try
                {
                    //lưu info vào cookie dạng accountId|userName|tokenKey|ServiceId_Old|billingAccessToken
                    if (HttpContext.Current.User.Identity.IsAuthenticated && HttpContext.Current.Request.ServerVariables["AUTH_USER"].ToString().Length > 0) //đã set form
                    {
                        var current = HttpContext.Current.Request.ServerVariables["AUTH_USER"].ToString();
                        var arrCurrent = current.ToString().Split('|');
                        if (arrCurrent.Length >= 4)
                        {
                            var accessToken = arrCurrent[2];
                            return accessToken;
                        }
                        else
                        {
                            return string.Empty;
                        }
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    NLogLogger.LogInfo(ex.Message);
                    return string.Empty;
                }
            }
        }
        public static string AccessTokenBilling
        {
            get
            {
                try
                {
                    //lưu info vào cookie dạng accountId|userName|tokenKey|ServiceId_Old|billingAccessToken
                    if (HttpContext.Current.User.Identity.IsAuthenticated && HttpContext.Current.Request.ServerVariables["AUTH_USER"].ToString().Length > 0) //đã set form
                    {
                        var current = HttpContext.Current.Request.ServerVariables["AUTH_USER"].ToString();
                        var arrCurrent = current.ToString().Split('|');
                        if (arrCurrent.Length >= 4)
                        {
                            string url = HttpContext.Current.Request.Url.AbsoluteUri;
                            if (HttpContext.Current.Session[GetNameSessionOrigin(url)] != null)
                            {
                                return HttpContext.Current.Session[GetNameSessionOrigin(url)].ToString();
                            }
                            else
                            {
                                return arrCurrent[4]; //trả về billing accesstoken trên form
                            }
                        }
                        else
                        {
                            return string.Empty;
                        }
                    }
                    else
                    {
                        return string.Empty;
                    }


                }
                catch (Exception ex)
                {
                    NLogLogger.LogInfo(ex.Message);
                    return string.Empty;
                }
            }
        }
        public static string AuthenService
        {
            get
            {
                try
                {
                    //lưu info vào cookie dạng accountId|userName|tokenKey|ServiceId_Old|billingAccessToken
                    if (HttpContext.Current.User.Identity.IsAuthenticated && HttpContext.Current.Request.ServerVariables["AUTH_USER"].ToString().Length > 0) //đã set form
                    {
                        var current = HttpContext.Current.Request.ServerVariables["AUTH_USER"].ToString();
                        var arrCurrent = current.ToString().Split('|');
                        if (arrCurrent.Length >= 4)
                        {
                            var accessToken = arrCurrent[3];
                            return accessToken;
                        }
                        else
                        {
                            return string.Empty;
                        }
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    NLogLogger.LogInfo(ex.Message);
                    return string.Empty;
                }
            }
        }
        public static string GetNameSessionOrigin(string linkOrigin)
        {

            string linkOriginRoot = GetUrlRootOfLink(linkOrigin);
            linkOriginRoot = linkOriginRoot.Replace("https://", "");
            linkOriginRoot = linkOriginRoot.Replace("http://", "");
            return linkOriginRoot;
        }
        public static string GetUrlRootOfLink(string linkOrigin)
        {
            string protocol = "";
            if (linkOrigin.Contains("https://"))
            {
                protocol = "https://";
            }
            else if (linkOrigin.Contains("http://"))
            {
                protocol = "http://";
            }
            var linkTemp = linkOrigin.Replace(protocol, "");
            linkTemp = linkTemp.Replace("www.", "");
            if (linkTemp.Contains("/"))
            {
                var arr = linkTemp.Split('/');
                linkTemp = arr[0];
            }
            var linkReturn = protocol + linkTemp;
            return linkReturn;

        }
    }
    /// <summary>
    /// Các tham số tracking
    /// </summary>
    public class SKDTrackingInfo
    {
        public string utm { get; set; }
        public string clientId { get; set; }
        public int deviceType { get; set; }
        public string deviceToken { get; set; }
        public string adsId { get; set; }
        public int activityType { get; set; }
        public string activityTypeExtend { get; set; }
        public string accountId { get; set; }
        public string accountName { get; set; }
        public int amount { get; set; }
    }
    /// <summary>
    /// Lib api tracking bên sdk
    /// </summary>
    public class SDKTrackApi
    {
        public static readonly string TrackingApi = ConfigurationManager.AppSettings["SKDTrackingApi"]?? "https://apisdk.vtcgame.vn/sdk/app/activity";
        public static void LogSDKTracking(SKDTrackingInfo param)
        {

            try
            {
                Action<SKDTrackingInfo> send = (SKDTrackingInfo data) =>
                {
                    _logSDKTracking(param);

                };
                send.BeginInvoke(param, null, null);
            }

            catch (Exception ex)
            {
                NLogLogger.PublishException(ex);

            }

        }
        private static void _logSDKTracking(SKDTrackingInfo trackingInfo)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(TrackingApi);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json =JsonConvert.SerializeObject(trackingInfo);

                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                NLogLogger.LogWarning("Log PU SDK: " + result);
            }
        }
    }
    /// <summary>
    /// Tập hợp lib api của phần người dùng
    /// </summary>
    public class AccountApi
    {
        public static readonly string Api = ConfigurationManager.AppSettings["Api"];

        /// <summary>
        /// Lấy thông tin người dùng đăng nhập vào hệ thống
        /// </summary>
        /// <returns>AccountDetail</returns>
        public static AccountDetail GetAccountDetail()
        {
            var link = Api + "account/detail";
            if (LoginConfig.AccountId > 0)
            {
                var ParamName = new string[] { "access_token", "account_name", "client_ip" };
                var ParamValue = new string[3];
                ParamValue[0] = LoginConfig.AccessToken;
                ParamValue[1] = LoginConfig.AccountName;
                ParamValue[2] = DBCommon.ClientIP;
                string responseDetail = HttpHelper.RestApiPost(link, ParamName, ParamValue);
                if (!string.IsNullOrEmpty(responseDetail) && responseDetail != "401")
                {
                    return JsonConvert.DeserializeObject<AccountDetail>(responseDetail);
                }
            }
            return null;
        }

        /// <summary>
        /// Kiểm tra thông tin xác thực mobile(status=1=> xác thực)
        /// </summary>
        /// <returns></returns>
        public static AccountMobile GetAccountMobile()
        {
            var link = Api + "account/mobile";
            if (LoginConfig.AccountId > 0)
            {
                var ParamName = new string[] { "access_token", "account_name", "client_ip", "account_id" };
                var ParamValue = new string[4];
                ParamValue[0] = LoginConfig.AccessToken;
                ParamValue[1] = LoginConfig.AccountName;
                ParamValue[2] = DBCommon.ClientIP;
                ParamValue[3] = LoginConfig.AccountId.ToString();
                //NLogLogger.LogDebug(string.Join(",", ParamValue));
                string responseDetail = HttpHelper.RestApiPost(link, ParamName, ParamValue);
                //NLogLogger.LogDebug(responseDetail);
                if (!string.IsNullOrEmpty(responseDetail) && responseDetail != "401")
                {
                    return JsonConvert.DeserializeObject<AccountMobile>(responseDetail);
                }
            }
            return null;
        }
        /// <summary>
        /// lấy thông tin smsplus
        /// </summary>
        /// <returns></returns>
        public static ApiListSmsPlus GetSmsPlusList()
        {
            var link = Api + "smsplus/list";
            if (LoginConfig.AccountId > 0)
            {
                var ParamName = new string[] { "access_token", "account_name", "client_ip", "account_id" };
                var ParamValue = new string[4];
                ParamValue[0] = LoginConfig.AccessToken;
                ParamValue[1] = LoginConfig.AccountName;
                ParamValue[2] = DBCommon.ClientIP;
                ParamValue[3] = LoginConfig.AccountId.ToString();
                string responseDetail = HttpHelper.RestApiPost(link, ParamName, ParamValue);
                if (!string.IsNullOrEmpty(responseDetail) && responseDetail != "401")
                {
                    return JsonConvert.DeserializeObject<ApiListSmsPlus>(responseDetail);
                }
            }
            return null;
        }
        /// <summary>
        /// Đổi email
        /// </summary>
        /// <param name="currentEmail"></param>
        /// <param name="newEmail"></param>
        /// <param name="answer"></param>
        /// <param name="question"></param>
        /// <param name="sign"></param>
        /// <returns></returns>
        public static InfoUpdate UpdateEmail(string currentEmail, string newEmail, string answer, int question, string sign)
        {
            var link = Api + "update/email";
            if (LoginConfig.AccountId > 0)
            {
                var ParamName = new string[] { "access_token", "account_name", "client_ip", "answer", "current_email", "new_email", "question_id", "sign" };
                var ParamValue = new string[8];
                ParamValue[0] = LoginConfig.AccessToken;
                ParamValue[1] = LoginConfig.AccountName;
                ParamValue[2] = DBCommon.ClientIP;
                ParamValue[3] = answer;
                ParamValue[4] = currentEmail;
                ParamValue[5] = StringUtility.RemoveScript(newEmail);
                ParamValue[6] = question.ToString();
                ParamValue[7] = sign;
                string responseDetail = HttpHelper.RestApiPost(link, ParamName, ParamValue);
                if (!string.IsNullOrEmpty(responseDetail) && responseDetail != "401")
                {
                    var info = JsonConvert.DeserializeObject<InfoUpdate>(responseDetail);
                    if (info != null && int.Parse(info.responseCode) <= 0)
                    {
                        NLogLogger.LogDebug(responseDetail);
                        NLogLogger.LogDebug("UpdateEmail Param: " + string.Join(",", ParamName) + " | " + string.Join(",", ParamValue));
                    }
                    return info;
                }
            }
            return null;
        }
        public static InfoUpdate UpdateQuestion(string answer, int question, string newAnswer, int newQuestion, string email, string mobile, string passport, string sign, string otp, int type)
        {
            var link = Api + "update/question";
            if (LoginConfig.AccountId > 0)
            {
                var ParamName = new string[] { "access_token", "account_name", "client_ip", "answer", "question_id", "new_answer", "new_question_id", "email", "mobile", "passport", "sign", "secure_code", "type" };
                var ParamValue = new string[ParamName.Length];
                ParamValue[0] = LoginConfig.AccessToken;
                ParamValue[1] = LoginConfig.AccountName;
                ParamValue[2] = DBCommon.ClientIP;
                ParamValue[3] = StringUtility.RemoveScript(answer);
                ParamValue[4] = StringUtility.RemoveScript(question.ToString());
                ParamValue[5] = StringUtility.RemoveScript(newAnswer);
                ParamValue[6] = newQuestion.ToString();
                ParamValue[7] = StringUtility.RemoveScript(email);
                ParamValue[8] = mobile;
                ParamValue[9] = passport;
                ParamValue[10] = sign;
                ParamValue[11] = otp;
                ParamValue[12] = type.ToString();
                string responseDetail = HttpHelper.RestApiPost(link, ParamName, ParamValue);
                if (!string.IsNullOrEmpty(responseDetail) && responseDetail != "401")
                {
                    var info = JsonConvert.DeserializeObject<InfoUpdate>(responseDetail);
                    if (info != null && int.Parse(info.responseCode) <= 0)
                    {
                        NLogLogger.LogDebug(responseDetail);
                        NLogLogger.LogDebug("UpdateQuestion Param: " + string.Join(",", ParamName) + " | " + string.Join(",", ParamValue));
                    }
                    return info;
                }
            }
            return null;
        }

        /// <summary>
        /// Xác thực số điện thoại
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="otp"></param>
        /// <param name="sign"></param>
        /// <returns></returns>
        public static InfoUpdate VerifyMobile(string mobile, string otp, string sign)
        {
            var link = Api + "verify/mobile";
            if (LoginConfig.AccountId > 0)
            {
                var ParamName = new string[] { "access_token", "account_name", "client_ip", "mobile", "otp", "sign" };
                var ParamValue = new string[6];
                ParamValue[0] = LoginConfig.AccessToken;
                ParamValue[1] = LoginConfig.AccountName;
                ParamValue[2] = DBCommon.ClientIP;
                ParamValue[3] = mobile;
                ParamValue[4] = otp;
                ParamValue[5] = sign;
                string responseDetail = HttpHelper.RestApiPost(link, ParamName, ParamValue);
                if (!string.IsNullOrEmpty(responseDetail) && responseDetail != "401")
                {
                    var info = JsonConvert.DeserializeObject<InfoUpdate>(responseDetail);
                    if (info != null && int.Parse(info.responseCode) <= 0)
                    {
                        NLogLogger.LogDebug("UpdateQuestion Param: " + string.Join(",", ParamName) + " | " + string.Join(",", ParamValue));
                        NLogLogger.LogDebug(responseDetail);
                    }
                    return info;
                }
            }
            return null;
        }
        /// <summary>
        /// Hủy xác thực
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="otp"></param>
        /// <param name="sign"></param>
        /// <returns></returns>
        public static InfoUpdate UnVerifyMobile(string mobile, string otp)
        {
            var link = Api + "unverify/mobile";
            if (LoginConfig.AccountId > 0)
            {
                var ParamName = new string[] { "access_token", "account_name", "client_ip", "mobile", "otp" };
                var ParamValue = new string[5];
                ParamValue[0] = LoginConfig.AccessToken;
                ParamValue[1] = LoginConfig.AccountName;
                ParamValue[2] = DBCommon.ClientIP;
                ParamValue[3] = mobile;
                ParamValue[4] = otp;
                // ParamValue[5] = sign;
                string responseDetail = HttpHelper.RestApiPost(link, ParamName, ParamValue);
                if (!string.IsNullOrEmpty(responseDetail) && responseDetail != "401")
                {
                    var info = JsonConvert.DeserializeObject<InfoUpdate>(responseDetail);
                    if (info != null && int.Parse(info.responseCode) <= 0)
                    {
                        NLogLogger.LogDebug(responseDetail);
                        NLogLogger.LogDebug("UnVerifyMobile Param: " + string.Join(",", ParamName) + " | " + string.Join(",", ParamValue));
                    }
                    return info;
                }
            }
            return null;
        }
        /// <summary>
        /// Đổi số điện thoại
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="otp"></param>
        /// <param name="new_mobile"></param>
        /// <param name="new_otp"></param>
        /// <param name="sign"></param>
        /// <returns></returns>
        public static InfoUpdate UpdateMobile(string mobile, string otp, string newMobile, string new_otp, string sign)
        {
            var link = Api + "update/mobile";
            if (LoginConfig.AccountId > 0)
            {
                var ParamName = new string[] { "access_token", "account_name", "client_ip", "mobile", "otp", "new_mobile", "new_otp", "sign" };
                var ParamValue = new string[8];
                ParamValue[0] = LoginConfig.AccessToken;
                ParamValue[1] = LoginConfig.AccountName;
                ParamValue[2] = DBCommon.ClientIP;
                ParamValue[3] = mobile;
                ParamValue[4] = otp;
                ParamValue[5] = newMobile;
                ParamValue[6] = new_otp;
                ParamValue[7] = sign;
                string responseDetail = HttpHelper.RestApiPost(link, ParamName, ParamValue);
                if (!string.IsNullOrEmpty(responseDetail) && responseDetail != "401")
                {
                    var info = JsonConvert.DeserializeObject<InfoUpdate>(responseDetail);
                    if (info != null && int.Parse(info.responseCode) <= 0)
                    {
                        NLogLogger.LogDebug(responseDetail);
                        NLogLogger.LogDebug("UpdateMobile Param: " + string.Join(",", ParamName) + " | " + string.Join(",", ParamValue));
                    }
                    return info;
                }
            }
            return null;
        }
        /// <summary>
        /// Cập nhật sms plus
        /// </summary>
        /// <param name="adv">Nhận tin nhắn QC</param>
        /// <param name="chudong"> Tin nhắn chủ động</param>
        /// <param name="desc"> Vcoin giảm</param>
        /// <param name="inc"> Vcon tăng</param>
        /// <param name="min_amount">Số Vcoin tối thiểu</param>
        /// <param name="payment">Truy vấn thông tin</param>
        /// <returns></returns>
        public static InfoUpdate UpdateSMSPlus(int adv, int chudong, int desc, int inc, int min_amount, int payment)
        {
            var link = Api + "smsplus/update";
            if (LoginConfig.AccountId > 0)
            {
                var ParamName = new string[] { "account_id", "account_name", "client_ip", "adv", "chudong", "code", "desc", "inc", "min_amount", "payment", "access_token" };
                var ParamValue = new string[11];

                ParamValue[0] = LoginConfig.AccountId.ToString();// account_id
                ParamValue[1] = LoginConfig.AccountName;//address
                ParamValue[2] = DBCommon.ClientIP;
                ParamValue[3] = adv.ToString();
                ParamValue[4] = chudong.ToString();
                ParamValue[5] = "";//code
                ParamValue[6] = desc.ToString();//desc
                ParamValue[7] = inc.ToString();//inc
                ParamValue[8] = min_amount.ToString();//min_amount
                ParamValue[9] = payment.ToString();//payment
                ParamValue[10] = LoginConfig.AccessToken.ToString();// account_id
                string responseDetail = HttpHelper.RestApiPost(link, ParamName, ParamValue);
                if (!string.IsNullOrEmpty(responseDetail) && responseDetail != "401")
                {
                    var info = JsonConvert.DeserializeObject<InfoUpdate>(responseDetail);
                    if (info != null && int.Parse(info.responseCode) <= 0)
                    {
                        NLogLogger.LogDebug(responseDetail);
                        NLogLogger.LogDebug("UpdateSMS Param: " + string.Join(",", ParamName) + " | " + string.Join(",", ParamValue));
                    }
                    return info;
                }
            }
            return null;
        }
        /// <summary>
        /// Kiểm tra tài khoản có đăng nhập bảo mật hay chưa( status=1=> đã có)
        /// </summary>
        /// <returns></returns>
        public static InfoUpdate CheckSecure()
        {
            var link = Api + "secure/info";
            if (LoginConfig.AccountId > 0)
            {
                var ParamName = new string[] { "access_token", "account_name", "client_ip" };
                var ParamValue = new string[3];
                ParamValue[0] = LoginConfig.AccessToken;
                ParamValue[1] = LoginConfig.AccountName;
                ParamValue[2] = DBCommon.ClientIP;
                string responseDetail = HttpHelper.RestApiPost(link, ParamName, ParamValue);
                //NLogLogger.LogDebug("secure/info:" + responseDetail);
                if (!string.IsNullOrEmpty(responseDetail) && responseDetail != "401")
                {
                    var info = JsonConvert.DeserializeObject<InfoUpdate>(responseDetail);
                    //if (info != null && int.Parse(info.responseCode) <= 0)
                    //{
                    //    NLogLogger.LogDebug(responseDetail);
                    //}
                    return info;
                }
            }
            return null;
        }
        /// <summary>
        /// Setup bảo mật đăng nhập
        /// </summary>
        /// <param name="otp"></param>
        /// <param name="secureType"></param>
        /// <param name="secureCode"></param>
        /// <param name="setupType"></param>
        /// <returns></returns>

        public static InfoUpdate AuthenSecure(int secureType, string secureCode, int setupType)
        {

            var link = Api + "secure/authen";
            if (LoginConfig.AccountId > 0)
            {
                var ParamName = new string[] { "account_name", "client_ip", "secure_code", "secure_type", "setup_type", "access_token" };
                var ParamValue = new string[6];


                ParamValue[0] = LoginConfig.AccountName;// account_id
                ParamValue[1] = DBCommon.ClientIP;
                ParamValue[2] = secureCode;
                ParamValue[3] = secureType.ToString();
                ParamValue[4] = setupType.ToString();//setup_type 1-Dang ky, 2- Huy
                ParamValue[5] = LoginConfig.AccessToken;
                string responseDetail = HttpHelper.RestApiPost(link, ParamName, ParamValue);
                if (!string.IsNullOrEmpty(responseDetail) && responseDetail != "401")
                {
                    var info = JsonConvert.DeserializeObject<InfoUpdate>(responseDetail);
                    if (info != null && int.Parse(info.responseCode) <= 0)
                    {
                        NLogLogger.LogDebug(responseDetail);
                        NLogLogger.LogDebug("SercurAuthen Param: " + string.Join(",", ParamName) + " | " + string.Join(",", ParamValue));
                    }
                    return info;
                }
            }
            return null;
        }
        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        /// <param name="password"></param>
        /// <param name="new_password"></param>
        /// <param name="secureType"></param>
        /// <param name="secureCode"></param>
        /// <returns></returns>
        public static InfoUpdate UpdatePassword(string password, string new_password, int secureType, string secureCode)
        {
            var link = Api + "update/password";
            if (LoginConfig.AccountId > 0)
            {

                var ParamName = new string[] { "account_name", "client_ip", "new_password", "old_password", "secure_code", "secure_type", "access_token" };
                var ParamValue = new string[7];
                ParamValue[0] = LoginConfig.AccountName;// account_id
                ParamValue[1] = DBCommon.ClientIP;
                ParamValue[2] = new_password;
                ParamValue[3] = password;
                ParamValue[4] = secureCode;
                ParamValue[5] = secureType.ToString();//
                ParamValue[6] = LoginConfig.AccessToken;
                string responseDetail = HttpHelper.RestApiPost(link, ParamName, ParamValue);
                if (!string.IsNullOrEmpty(responseDetail) && responseDetail != "401")
                {
                    var info = JsonConvert.DeserializeObject<InfoUpdate>(responseDetail);
                    if (info != null && int.Parse(info.responseCode) <= 0)
                    {
                        NLogLogger.LogDebug(responseDetail);
                        //NLogLogger.LogDebug("UpdatePassword Param: " + string.Join(",", ParamName) + " | " + string.Join(",", ParamValue));
                    }
                    return info;
                }
            }
            return null;
        }
        /// <summary>
        /// Mở đóng băng Vcoin
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="secureType"></param>
        /// <param name="secureCode"></param>
        /// <returns></returns>
        public static InfoUpdate UnFreeze(int amount, int secureType, string secureCode)
        {
            var link = Api + "vcoin/unfreeze";
            if (LoginConfig.AccountId > 0)
            {

                var ParamName = new string[] { "account_id", "client_ip", "secure_code", "secure_type", "vcoin", "access_token", "account_name" };
                var ParamValue = new string[7];

                ParamValue[0] = LoginConfig.AccountId.ToString();// account_id
                ParamValue[1] = DBCommon.ClientIP;
                ParamValue[2] = secureCode;
                ParamValue[3] = secureType.ToString();
                ParamValue[4] = amount.ToString();
                ParamValue[5] = LoginConfig.AccessToken;
                ParamValue[6] = LoginConfig.AccountName;
                string responseDetail = HttpHelper.RestApiPost(link, ParamName, ParamValue);
                if (!string.IsNullOrEmpty(responseDetail) && responseDetail != "401")
                {
                    var info = JsonConvert.DeserializeObject<InfoUpdate>(responseDetail);
                    if (info != null && int.Parse(info.responseCode) <= 0)
                    {
                        NLogLogger.LogDebug(responseDetail);
                        NLogLogger.LogDebug("Unfreeze Param: " + string.Join(",", ParamName) + " | " + string.Join(",", ParamValue));
                    }
                    return info;
                }
            }
            return null;
        }
        /// <summary>
        /// Đóng băng vcoin
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static InfoUpdate Freeze(int amount)
        {
            var link = Api + "vcoin/freeze";
            if (LoginConfig.AccountId > 0)
            {

                var ParamName = new string[] { "account_id", "client_ip", "vcoin", "access_token", "account_name" };
                var ParamValue = new string[5];

                ParamValue[0] = LoginConfig.AccountId.ToString();// account_id
                ParamValue[1] = DBCommon.ClientIP;
                ParamValue[2] = amount.ToString();
                ParamValue[3] = LoginConfig.AccessToken;
                ParamValue[4] = LoginConfig.AccountName;
                string responseDetail = HttpHelper.RestApiPost(link, ParamName, ParamValue);
                if (!string.IsNullOrEmpty(responseDetail) && responseDetail != "401")
                {
                    var info = JsonConvert.DeserializeObject<InfoUpdate>(responseDetail);
                    if (info != null && int.Parse(info.responseCode) <= 0)
                    {
                        NLogLogger.LogDebug(responseDetail);
                        NLogLogger.LogDebug("Freeze Param: " + string.Join(",", ParamName) + " | " + string.Join(",", ParamValue));
                    }
                    return info;
                }
            }
            return null;
        }
        public static Balance GetBalance()
        {
            var link = Api + "balance/all";
            if (LoginConfig.AccountId > 0)
            {

                var ParamName = new string[] { "account_id", "client_ip", "access_token", "account_name" };
                var ParamValue = new string[4];

                ParamValue[0] = LoginConfig.AccountId.ToString();// account_id
                ParamValue[1] = DBCommon.ClientIP;
                ParamValue[2] = LoginConfig.AccessToken;
                ParamValue[3] = LoginConfig.AccountName;
                string responseDetail = HttpHelper.RestApiPost(link, ParamName, ParamValue);
                if (!string.IsNullOrEmpty(responseDetail) && responseDetail != "401")
                {
                    var info = JsonConvert.DeserializeObject<Balance>(responseDetail);

                    return info;
                }
            }
            return null;
        }
        public static TransferPolicyResponseData GetTransferVcoinPolicy(int amount)
        {
            var link = Api + "transfer/vcoinpolicy";
            if (LoginConfig.AccountId > 0)
            {

                var ParamName = new string[] { "transfer_account_id", "vcoin", "access_token", "transfer_account_name", "account_name" };
                var ParamValue = new string[5];

                ParamValue[0] = LoginConfig.AccountId.ToString();// account_id
                ParamValue[1] = amount.ToString();
                ParamValue[2] = LoginConfig.AccessToken;
                ParamValue[3] = LoginConfig.AccountName;
                ParamValue[4] = LoginConfig.AccountName;
                string responseDetail = HttpHelper.RestApiPost(link, ParamName, ParamValue);
                if (!string.IsNullOrEmpty(responseDetail) && responseDetail != "401")
                {
                    var info = JsonConvert.DeserializeObject<TransferPolicyResponseData>(responseDetail);
                    //if (info != null && int.Parse(info.responseCode) <= 0)
                    //{
                    //    NLogLogger.LogDebug(responseDetail);
                    //}
                    return info;
                }
            }
            return null;
        }
        /// <summary>
        /// Lấy mật khẩu qua otp
        /// </summary>
        /// <param name="password"></param>
        /// <param name="secureType"></param>
        /// <param name="secureCode"></param>
        /// <returns></returns>
        public static InfoUpdate ResetPasswordByOtp(string accountName, string password, int secureType, string secureCode)
        {
            var link = Api + "reset/passwordotp";


            var ParamName = new string[] { "account_name", "client_ip", "password", "secure_code", "secure_type" };
            var ParamValue = new string[5];
            ParamValue[0] = accountName;// account_id
            ParamValue[1] = DBCommon.ClientIP;
            ParamValue[2] = password;
            ParamValue[3] = secureCode;
            ParamValue[4] = secureType.ToString();//

            string responseDetail = HttpHelper.RestApiPost(link, ParamName, ParamValue);
            if (!string.IsNullOrEmpty(responseDetail) && responseDetail != "401")
            {
                var info = JsonConvert.DeserializeObject<InfoUpdate>(responseDetail);
                if (info != null && int.Parse(info.responseCode) <= 0)
                {
                    NLogLogger.LogDebug(responseDetail);
                    NLogLogger.LogDebug("ResetPassOTP Param: " + string.Join(",", ParamName) + " | " + string.Join(",", ParamValue));
                }
                return info;
            }

            return null;
        }
        /// <summary>
        /// Lấy mật khẩu qua email
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="password"></param>
        /// <param name="email"></param>
        /// <param name="sign"></param>
        /// <returns></returns>
        public static InfoUpdate ResetPasswordByMail(string accountName, string password, string email, string sign)
        {
            var link = Api + "reset/passwordemail";


            var ParamName = new string[] { "account_name", "client_ip", "password", "email", "sign" };
            var ParamValue = new string[5];
            ParamValue[0] = accountName;// account_id
            ParamValue[1] = DBCommon.ClientIP;
            ParamValue[2] = password;
            ParamValue[3] = email;
            ParamValue[4] = sign;//

            string responseDetail = HttpHelper.RestApiPost(link, ParamName, ParamValue);
            if (!string.IsNullOrEmpty(responseDetail) && responseDetail != "401")
            {
                var info = JsonConvert.DeserializeObject<InfoUpdate>(responseDetail);
                if (info != null && int.Parse(info.responseCode) <= 0)
                {
                    NLogLogger.LogDebug(responseDetail);
                    NLogLogger.LogDebug("ResetPassbyEmail Param: " + string.Join(",", ParamName) + " | " + string.Join(",", ParamValue));
                }
                return info;
            }

            return null;
        }

        /// <summary>
        /// Hàm insert thông tin profile đói với người dùng chưa đủ thông tin
        /// </summary>
        /// <param name="email">Email</param>
        /// <param name="phone">điện thoại</param>
        /// <param name="passport">số chứng mình nhân đân</param>
        /// <param name="fullName">tên đầy đủ</param>
        /// <param name="birthday">ngày sinh nhận</param>
        /// <param name="gender">Giới tính</param>
        /// <param name="address">địa chỉ</param>
        /// <param name="questionsId">câu hỏi bảo mật</param>
        /// <param name="answer">câu trả lời</param>
        /// <param name="secureCode">mã bảo mật</param>
        /// <param name="sign">chữ ký</param>
        /// <returns>InfoUpdate</returns>
        public static InfoUpdate InsertProfile(string email, string phone, string passport, string fullName, DateTime birthday, string gender, string address, string questionsId, string answer, string secureCode, string sign, string cityId = "1", string districtId = "1", string wardId = "1")
        {
            string insertUrl = Api + "insert/profile";
            string[] paramName = new string[] { "access_token", "account_name", "email", "mobile", "passport", "full_name", "birthday", "gender", "location_id", "district_id", "ward_id", "address", "question_id", "answer", "client_ip", "secure_code", "sign" };
            var ParamValue = new string[paramName.Length];
            ParamValue[0] = LoginConfig.AccessToken;
            ParamValue[1] = LoginConfig.AccountName;
            ParamValue[2] = StringUtility.RemoveScript(email);
            ParamValue[3] = phone;
            ParamValue[4] = passport;
            ParamValue[5] = StringUtility.RemoveScript(fullName);
            ParamValue[6] = DateUtilities.ConvertDateTimeToTimeStamp(birthday).ToString();
            ParamValue[7] = gender;
            ParamValue[8] = cityId;
            ParamValue[9] = districtId;
            ParamValue[10] = wardId;
            ParamValue[11] = StringUtility.RemoveScript(address);
            ParamValue[12] = questionsId;
            ParamValue[13] = answer;
            ParamValue[14] = DBCommon.ClientIP;
            ParamValue[15] = secureCode;
            ParamValue[16] = sign;

            string insertPostInfo = new API().PostURL(insertUrl, paramName, ParamValue);
            if (!string.IsNullOrEmpty(insertPostInfo))
            {
                //return JsonConvert.DeserializeObject<InfoUpdate>(insertPostInfo);
                var info = JsonConvert.DeserializeObject<InfoUpdate>(insertPostInfo);
                if (info != null && int.Parse(info.responseCode) <= 0)
                {
                    NLogLogger.LogDebug(insertPostInfo);
                    NLogLogger.LogDebug("InserProfile Param: " + string.Join(",", paramName) + " | " + string.Join(",", ParamValue));
                }
                return info;
            }
            return null;
        }

        /// <summary>
        /// Hàm update thông tin profile đói với người dùng đủ thông tin
        /// </summary>
        /// <param name="address">Địa chỉ</param>
        /// <param name="fullName">Tên đầy đủ</param>
        /// <param name="gender">Giới tính</param>
        /// <param name="mobile">Điện thoại</param>
        /// <param name="birthday">Ngày sinh</param>
        /// <returns>InfoUpdate</returns>
        public static InfoUpdate UpdateProfile(string address, string fullName, string gender, string mobile, DateTime birthday, string cityId = "1", string districtId = "1", string wardId = "1")
        {
            string updateUrl = Api + "update/profile";
            string[] paramName = new string[] { "account_id", "address", "birthday", "client_ip", "full_name", "gender", "mobile", "location_id", "district_id", "ward_id", "access_token" };
            var ParamValue = new string[paramName.Length];
            ParamValue[0] = LoginConfig.AccountId.ToString();
            ParamValue[1] = StringUtility.RemoveScript(address);
            ParamValue[2] = DateUtilities.ConvertDateTimeToTimeStamp(birthday).ToString();
            ParamValue[3] = DBCommon.ClientIP;
            ParamValue[4] = StringUtility.RemoveScript(fullName);
            ParamValue[5] = gender.ToString();
            ParamValue[6] = mobile;
            ParamValue[7] = cityId;
            ParamValue[8] = districtId;
            ParamValue[9] = wardId;
            ParamValue[10] = LoginConfig.AccessToken;
            NLogLogger.LogDebug(string.Join(",", ParamValue));
            string insertPostInfo = new API().PostURL(updateUrl, paramName, ParamValue);
            if (!string.IsNullOrEmpty(insertPostInfo))
            {
                var info = JsonConvert.DeserializeObject<InfoUpdate>(insertPostInfo);
                if (info != null && int.Parse(info.responseCode) <= 0)
                {
                    NLogLogger.LogDebug(insertPostInfo);
                    NLogLogger.LogDebug("UpdateProfile Param: " + string.Join(",", paramName) + " | " + string.Join(",", ParamValue));
                }
                return info;
            }
            return null;
        }
        public static List<TransHistory> GetHistoryTransaction(DateTime dateFrom, DateTime dateTo)
        {
            var link = Api + "transaction/history";
            if (LoginConfig.AccountId > 0)
            {
                var from_date_Milisecond = DateUtilities.ConvertDateTimeToTimeStamp(dateFrom);
                var to_date_Milisecond = DateUtilities.ConvertDateTimeToTimeStamp(dateTo);
                var ParamNameVcoin = new string[] { "account_id", "service_id", "from_date", "to_date", "client_ip", "access_token", "account_name" };
                var ParamValueVcoin = new string[7];

                ParamValueVcoin[0] = LoginConfig.AccountId.ToString();// account_id
                ParamValueVcoin[1] = "0"; //ConfigurationManager.AppSettings["SERVICE_ID"];
                ParamValueVcoin[2] = from_date_Milisecond.ToString();//address
                ParamValueVcoin[3] = to_date_Milisecond.ToString();
                ParamValueVcoin[4] = VTCGame.Utility.DBCommon.ClientIP;
                ParamValueVcoin[5] = LoginConfig.AccessToken;
                ParamValueVcoin[6] = LoginConfig.AccountName;


                string responseDetail = HttpHelper.RestApiPost(link, ParamNameVcoin, ParamValueVcoin);
                if (!string.IsNullOrEmpty(responseDetail) && responseDetail != "401")
                {
                    //NLogLogger.LogDebug(responseDetail);
                    //NLogLogger.LogDebug("GetHistoryTransaction Param: " + string.Join(",", ParamNameVcoin) + " | " + string.Join(",", ParamValueVcoin));
                    var history = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TransHistory>>(responseDetail);
                    return history;
                }
            }
            return null;
        }
        public static TransferRequestData Transfer(int vcoin, string receive_account_name, long transId, long client_time, string secureCode, string description, string sign)
        {
            string insertUrl = Api + "transfer/vcoin";
            string[] paramName = new string[] { "access_token", "account_name", "client_ip", "client_time", "created_user_id", "created_user_name", "description", "device_type", "receive_account_name", "related_transaction_id", "secure_code", "service_id", "sign", "transfer_account_name", "vcoin" };
            var ParamValue = new string[paramName.Length];
            ParamValue[0] = LoginConfig.AccessToken;
            ParamValue[1] = LoginConfig.AccountName;
            ParamValue[2] = DBCommon.ClientIP;
            ParamValue[3] = client_time.ToString();
            ParamValue[4] = LoginConfig.AccountId.ToString();
            ParamValue[5] = LoginConfig.AccountName; ;
            ParamValue[6] = StringUtility.RemoveScript(description);
            ParamValue[7] = "1";
            ParamValue[8] = receive_account_name;
            ParamValue[9] = transId.ToString();
            ParamValue[10] = secureCode;
            ParamValue[11] = ConfigurationManager.AppSettings["TRANFER_SERVICE_ID"];
            ParamValue[12] = sign;
            ParamValue[13] = LoginConfig.AccountName;
            ParamValue[14] = vcoin.ToString();

            string insertPostInfo = new API().PostURL(insertUrl, paramName, ParamValue);
            if (!string.IsNullOrEmpty(insertPostInfo))
            {
                //return JsonConvert.DeserializeObject<InfoUpdate>(insertPostInfo);
                var info = JsonConvert.DeserializeObject<TransferRequestData>(insertPostInfo);
                if (info != null && info.responseCode <= 0)
                {
                    NLogLogger.LogDebug(insertPostInfo);
                    NLogLogger.LogDebug("Tranfer Param: " + string.Join(",", paramName) + " | " + string.Join(",", ParamValue));
                }
                return info;
            }
            return null;
        }


        /// <summary>
        /// Lấy danh sách huyện
        /// </summary>
        /// <param name="locationId"></param>
        /// <returns></returns>
        private static List<DistrictInfo> GetDistrict(int locationId)
        {
            var link = Api + "utils/districts";
            var ParamNameVcoin = new string[] { "location_id" };
            var ParamValueVcoin = new string[1];
            ParamValueVcoin[0] = locationId.ToString();// locationId
            string responseDetail = HttpHelper.RestApiPost(link, ParamNameVcoin, ParamValueVcoin);
            if (!string.IsNullOrEmpty(responseDetail) && responseDetail != "401")
            {

                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DistrictInfo>>(responseDetail);
                return data;
            }
            return null;
        }
        public static List<DistrictInfo> GetDistrictCache(int locationId)
        {
            var data = new List<DistrictInfo>();
            data = (List<DistrictInfo>)DataCaching.GetCache("GetDistrict" + locationId);
            if (data == null)
            {
                data = GetDistrict(locationId);
                if (data != null)
                {
                    DataCaching.InsertCache("GetDistrict" + locationId, data, 1440);
                }
            }
            return data;
        }
        /// <summary>
        /// Lấy danh sách tỉnh
        /// </summary>
        /// <returns></returns>
        private static List<LocationInfo> GetLocation()
        {
            var link = Api + "utils/locations";
            var ParamNameVcoin = new string[] { "national_id" };
            var ParamValueVcoin = new string[1];
            ParamValueVcoin[0] = "4";// locationId
            string responseDetail = HttpHelper.RestApiPost(link, ParamNameVcoin, ParamValueVcoin);
            if (!string.IsNullOrEmpty(responseDetail) && responseDetail != "401")
            {

                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<LocationInfo>>(responseDetail);
                return data;
            }
            return null;
        }
        public static List<LocationInfo> GetLocationCache()
        {
            var data = new List<LocationInfo>();
            data = (List<LocationInfo>)DataCaching.GetCache("GetLocation");
            if (data == null)
            {
                data = GetLocation();
                if (data != null)
                {
                    DataCaching.InsertCache("GetLocation", data, 1440);
                }
            }
            return data;
        }
        /// <summary>
        /// Lấy danh sách phường
        /// </summary>
        /// <param name="district_id"></param>
        /// <returns></returns>
        public static List<WardInfo> GetWard(int districtId)
        {
            var link = Api + "utils/wards";
            var ParamNameVcoin = new string[] { "district_id" };
            var ParamValueVcoin = new string[1];
            ParamValueVcoin[0] = districtId.ToString();// locationId
            string responseDetail = HttpHelper.RestApiPost(link, ParamNameVcoin, ParamValueVcoin);
            if (!string.IsNullOrEmpty(responseDetail) && responseDetail != "401")
            {

                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WardInfo>>(responseDetail);
                return data;
            }
            return null;
        }

        private static List<QuestionInfo> GetQuestion()
        {
            var link = Api + "utils/questions";
            var ParamNameVcoin = new string[0];
            var ParamValueVcoin = new string[0];

            string responseDetail = HttpHelper.RestApiPost(link, ParamNameVcoin, ParamValueVcoin);
            if (!string.IsNullOrEmpty(responseDetail) && responseDetail != "401")
            {

                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<QuestionInfo>>(responseDetail);
                return data;
            }
            return null;
        }
        public static List<QuestionInfo> GetQuestionCache()
        {
            var data = new List<QuestionInfo>();
            data = (List<QuestionInfo>)DataCaching.GetCache("GetQuestion");
            if (data == null)
            {
                data = GetQuestion();
                if (data != null)
                {
                    DataCaching.InsertCache("GetQuestion", data, 1440);
                }
            }
            return data;
        }
    }
}