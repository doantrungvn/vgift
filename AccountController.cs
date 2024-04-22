using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using VTCGame.DataAccess.DAOImpl;
using VTCGame.DataAccess.DTO;
using VTCGame.DataAccess.Factory;
using VTCGame.Portal.Models;
using VTCGame.Utility;
using VTCGame.Utility.Email;
using VTCGame.Utility.Security;
namespace VTCGame.Portal.Controllers
{
    public class AccountController : BaseController
    {
        /// <summary>
        /// Thông tin tài khoản
        /// </summary>
        /// <param name="type">
        /// 1 Tab thông tin chung
        /// 2 Tab vip
        /// 3 Bảo mật tài khoản
        /// 4 Tin nhắn
        /// 5 Nhiệm vụ
        /// 6 Lịch sử giao dịch
        /// 7 Hỗ trợ
        /// </param>
        /// <returns></returns>
        public ActionResult Index(int type, int subtype = 0)
        {
            if (type == 5)// trường hợp vào nhiệm vụ thì ra trang chủ
            {
                return Redirect("https://vtcgame.vn/");
            }

            var CookieAuthenType = Request.Cookies["LastLogin"];
            if (CookieAuthenType == null)
            {
                var accountId = LoginConfig.AccountId;
                var accoutName = LoginConfig.AccountName;
                if (accountId > 0)
                {
                    // set lastlogin để lấy thông tin người dùng cho shopvip và vòng quay
                    var responStatus = 0;
                    AbstractDAOFactory.Instance().CreateNewGiftShopDAO().WEB__Accounts_SetLastLoginTime(accountId, accoutName, out responStatus);
                    if (responStatus < 0)
                    {
                        NLogLogger.DebugMessage($"Lỗi set lastlogin: accountId: {accountId}, accountName: {accoutName}");
                    }
                    else
                    {
                        HttpCookie LastLogin = new HttpCookie("LastLogin");
                        LastLogin.Domain = ".vtcgame.vn";
                        LastLogin.Value = "1";
                        Response.Cookies.Add(LastLogin);
                    }
                }
            }

            ViewBag.Type = type;
            ViewBag.subtype = subtype;

            //Check đến trường hợp truy cập là mobile thì chuyển sang trang mobile
            if (IsMobileVersion)
                return View("Wap_Index", new AccountModel { Type = type, SubType = subtype });

            return View();
        }

        #region Tab Thông tin cá nhân
        public PartialViewResult GeneralInfo()
        {
            var accDetail = AccountApi.GetAccountDetail();
            if (accDetail == null)
            {
                FormsAuthentication.SignOut();
                SessionsManager.RemoveAllCookie();
                Session.Abandon();
                Response.Redirect(Config.UrlRoot);
            }

            ViewBag.IsInsertModel = accDetail.userStatus == 1 && string.IsNullOrEmpty(accDetail.email);//0: chưa kích hoạt;1: đang hoạt động;2: không hoạt động;3: khoá toàn hệ thống;4: khoá 1 vài dịch vụ
            ViewBag.IsActiveMobile = false;
            var mobileInfo = AccountApi.GetAccountMobile();
            if (mobileInfo != null)
            {
                ViewBag.IsActiveMobile = mobileInfo.status == 1 || mobileInfo.status == 4;
            }

            //Check đến trường hợp truy cập là mobile thì chuyển sang trang mobile
            if (IsMobileVersion)
                return PartialView("Wap_GeneralInfo", accDetail);

            return PartialView(accDetail);
        }

        /// <summary>
        /// Action này sử dụng khi người dùng update thông tin lân đầu tiên
        /// Phải updaet đủ Email, SĐT, Họ tên, ngày sinh, giới tính, câu hỏi bảo mật và câu trả lời, địa chỉ.
        /// Trong trường hợp người dùng đã kích hoạt số điện thoại thì cần gửi lên thêm 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult InsertInfo()
        {
            int ResponseStatus = -100;
            string ErorrMess = "lỗi mặc định";
            if (LoginConfig.AccountId > 0)
            {
                //Kiểm tra xem thông tin đã đủ chưa. Nếu đủ rồi thì chỉ cho update một số case Cơ bản
                AccountDetail accDetail = AccountApi.GetAccountDetail();
                if (accDetail == null)
                {
                    return Json(new { ResponseStatus = ResponseStatus, ErorrMess = ErorrMess });
                }
                if (accDetail.userStatus == 1 && string.IsNullOrEmpty(accDetail.email))
                {
                    // trạng thái insert
                    string fullName = Request["fullName"];
                    string passport = Request["passport"];
                    string phone = Request["phone"];
                    string email = Request["email"];
                    string gender = Request["gender"];
                    string dayOfBirth = Request["dayOfBirth"];
                    string monthOfBirth = Request["monthOfBirth"];
                    string yearOfBirth = Request["yearOfBirth"];
                    string address = Request["address"];
                    string questionsId = Request["questionsId"];
                    string answer = Request["answer"];
                    string cityId = Request["cbcityId"];
                    string districtId = Request["cbdistrictId"];
                    string wardId = Request["cbwardId"];
                    string otp = Request["otp"];
                    var birthday = new DateTime(int.Parse(yearOfBirth), int.Parse(monthOfBirth), int.Parse(dayOfBirth));

                    //Valida dữ liệu
                    bool dataIsInvalid = string.IsNullOrEmpty(passport) || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(questionsId) || string.IsNullOrEmpty(answer);
                    if (dataIsInvalid)
                    {
                        return Json(new { ResponseStatus = -50, ErorrMess = "Nhập thiếu thông tin người dùng." });
                    }

                    var dataPostBack = AccountApi.InsertProfile(email, phone, passport, fullName, birthday, gender, address, questionsId, answer, otp, "", cityId, districtId, wardId);
                    if (dataPostBack == null)
                    {
                        FormsAuthentication.SignOut();
                        SessionsManager.RemoveAllCookie();
                        Session.Abandon();
                        return Json(new { ResponseStatus = -77, ErorrMess = "Hết phiên làm việc" });
                    }
                    if (!string.IsNullOrEmpty(dataPostBack.responseCode))
                    {
                        //> 0: Thành công
                        //-1: Email không hợp lệ
                        //-2: Email đã tồn tại
                        //-3: Sđt ko hợp lệ
                        //-4: Số điện thoại đã tồn tại
                        //-7: Mã xác thực ko hợp lệ
                        //-50: Tài khoản ko tồn tại
                        //- 600: Dữ liệu không hợp lệ hoặc để trống
                        //#: Lỗi chưa định nghĩa
                        ResponseStatus = int.Parse(dataPostBack.responseCode);
                        ErorrMess = dataPostBack.description;
                        if (ResponseStatus == 1)
                        {
                            //Ghi thông tin thay đổi vào db
                            var jsonObject = new AccountInfoJson()
                            {
                                Address = address,
                                Answer = answer,
                                DayOfBirth = dayOfBirth,
                                Email = email,
                                FullName = fullName,
                                Gender = gender,
                                MonthOfBirth = monthOfBirth,
                                Passport = passport,
                                Phone = phone,
                                QuestionsId = questionsId,
                                YearOfBirth = yearOfBirth,
                                CityId = cityId,
                                DistrictId = districtId,
                                WardId = wardId,

                            };
                            AbstractDAOFactory.Instance().CreateAccountInfoTempDAO().Insert(dataPostBack.extend, JsonConvert.SerializeObject(jsonObject));
                            var keyBase64 = Security.Base64Encode(dataPostBack.extend);
                            try
                            {
                                using (var mailService = new EBankMailService.MailAPISoapClient())
                                {
                                    var pra = $"@Username={accDetail.id};@Link={DBCommon.UrlRoot}/verifyinsertinfo/{keyBase64}";
                                    // NLogLogger.LogInfo(pra + "Mail Id : 2");
                                    var result = mailService.SendMaileBankPaygate(email, accDetail.userName, accDetail.id, 2, pra);
                                    //if (result < 1)
                                    NLogLogger.Info(string.Format("SendMail InsertProfile Email result:{0}, newemail:{1}, AccountName: {2}", result, email, LoginConfig.AccountName));
                                }
                            }
                            catch (Exception ex)
                            {
                                NLogLogger.Info(ex.ToString());
                            }
                        }
                    }
                }
                else
                {
                    //Trạng thái update thông tin cơ bản
                    string fullName = Request["fullName"];
                    string gender = Request["gender"];
                    string phone = Request["phone"];
                    string dayOfBirth = Request["dayOfBirth"];
                    string monthOfBirth = Request["monthOfBirth"];
                    string yearOfBirth = Request["yearOfBirth"];
                    string address = Request["address"];
                    string cityId = Request["cbcityId"];
                    string districtId = Request["cbdistrictId"];
                    string wardId = Request["cbwardId"];
                    var birthday = new DateTime(int.Parse(yearOfBirth), int.Parse(monthOfBirth), int.Parse(dayOfBirth));

                    //Valida dữ liệu
                    bool dataIsInvalid = string.IsNullOrEmpty(fullName);
                    if (dataIsInvalid)
                    {
                        return Json(new { ResponseStatus = -50, ErorrMess = "Nhập thiếu thông tin người dùng." });
                    }

                    var postDataApi = AccountApi.UpdateProfile(address, fullName, gender, phone, birthday, cityId, districtId, wardId);
                    if (postDataApi == null)
                    {
                        FormsAuthentication.SignOut();
                        SessionsManager.RemoveAllCookie();
                        Session.Abandon();
                        return Json(new { ResponseStatus = -77, ErorrMess = "Hết phiên làm việc" });
                    }
                    if (!string.IsNullOrEmpty(postDataApi.responseCode))
                    {
                        ResponseStatus = int.Parse(postDataApi.responseCode);
                        ErorrMess = postDataApi.description;
                    }
                }
            }
            return Json(new { ResponseStatus = ResponseStatus, ErorrMess = ErorrMess });
        }

        /// <summary>
        /// Xác nhận thay đổi người dùng
        /// Chỉ dùng trong trường hợp người dùng kích hoạt qua e mail
        /// </summary>
        /// <returns></returns>
        public ActionResult VerifyInsertInfo(string key)
        {
            //chưa đăng nhập thì yêu cầu đăng nhập rồi trả về trang xác nhận
            if (LoginConfig.AccountId <= 0)
            {
                return Redirect($"{DBCommon.UrlRoot}?returnUrl={DBCommon.UrlRoot}verifyinsertinfo/{key}&type=login");
            }

            if (!string.IsNullOrEmpty(key))
            {
                var dbKey = Security.Base64Decode(key);
                var accountInfoTemp = AbstractDAOFactory.Instance().CreateAccountInfoTempDAO().GetById(dbKey);
                if (accountInfoTemp != null && accountInfoTemp.CreatedTime < DateTime.Now.AddDays(7) && !accountInfoTemp.Used)//giới hạn trong vòn 1 tuần
                {
                    var infoJsonPaser = JsonConvert.DeserializeObject<AccountInfoJson>(accountInfoTemp.AccountInfo);


                    var year = 2016;
                    var month = 1;
                    var day = 1;
                    int.TryParse(infoJsonPaser.YearOfBirth, out year);
                    int.TryParse(infoJsonPaser.MonthOfBirth, out month);
                    int.TryParse(infoJsonPaser.DayOfBirth, out day);
                    var birthday = new DateTime(year, month, day);

                    var insertPostInfo = AccountApi.InsertProfile(infoJsonPaser.Email, infoJsonPaser.Phone, infoJsonPaser.Passport, infoJsonPaser.FullName, birthday, infoJsonPaser.Gender, infoJsonPaser.Address, infoJsonPaser.QuestionsId, infoJsonPaser.Answer, "", accountInfoTemp.Sign, infoJsonPaser.CityId, infoJsonPaser.DistrictId, infoJsonPaser.WardId);

                    if (insertPostInfo == null)
                    {
                        FormsAuthentication.SignOut();
                        SessionsManager.RemoveAllCookie();
                        Session.Abandon();
                        Response.Redirect(Config.UrlRoot);
                        return Redirect($"{DBCommon.UrlRoot}?returnUrl={DBCommon.UrlRoot}verifyinsertinfo/{key}");
                    }


                    if (insertPostInfo.responseCode == "1")
                    {
                        AbstractDAOFactory.Instance().CreateAccountInfoTempDAO().SetUse(dbKey);//xác nhận đã xử dụng link
                        return Redirect(DBCommon.UrlRoot + "thong-tin-tai-khoan?activeinfo=1");
                    }
                    return Redirect(DBCommon.UrlRoot + "thong-tin-tai-khoan?activeinfo=0");
                }
            }
            return Redirect(DBCommon.UrlRoot);//trở ra trang chủ nếu không có key
        }
        #endregion

        #region Đổi Thay đổi, xóa SDT
        public PartialViewResult PopupVerifyPhone(string accuntName, string currentPhone)
        {
            ViewBag.AccountName = accuntName;
            ViewBag.CurrentPhone = currentPhone;
            return PartialView();
        }

        public PartialViewResult PopupUnVerifyPhone(string accuntName, string currentPhone)
        {
            ViewBag.AccountName = accuntName;
            ViewBag.CurrentPhone = currentPhone;
            return PartialView();
        }

        public PartialViewResult PopupChangePhone(string accuntName, string currentPhone)
        {
            ViewBag.AccountName = accuntName;
            ViewBag.CurrentPhone = currentPhone;
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ChangePhone()
        {
            int ResponseStatus = -100;
            string ErorrMess = "Hệ thống đang bận, bạn vui lòng quay lại sau";

            if (LoginConfig.AccountId > 0)
            {
                string currentPhone = Request["currentPhone"];
                string newPhone = Request["newPhone"];
                string currentOtp = Request["currentOtp"];
                string newOtp = Request["newOtp"];
                string flow = Request["flow"];

                if (!string.IsNullOrEmpty(flow))
                {
                    if (flow == "1")//call bước 1
                    {
                        bool isInvalid = string.IsNullOrEmpty(currentPhone) || string.IsNullOrEmpty(newPhone) || string.IsNullOrEmpty(currentOtp);
                        if (isInvalid)
                        {
                            return Json(new { ResponseStatus = -99, ErorrMess = "Thông tin không hợp lệ." });
                        }

                        var reruls = AccountApi.UpdateMobile(currentPhone, currentOtp, newPhone, "", "");
                        if (reruls == null)
                        {
                            FormsAuthentication.SignOut();
                            SessionsManager.RemoveAllCookie();
                            Session.Abandon();
                            return Json(new { ResponseStatus = -77, ErorrMess = "Hết phiên làm việc" });
                        }
                        return Json(new { ResponseStatus = reruls.responseCode, ErorrMess = reruls.description, sign = reruls.extend });
                    }
                    else//call bước 2
                    {
                        bool isInvalid = string.IsNullOrEmpty(currentPhone) || string.IsNullOrEmpty(newPhone) || string.IsNullOrEmpty(currentOtp) || string.IsNullOrEmpty(currentOtp) || string.IsNullOrEmpty(newOtp);
                        if (isInvalid)
                        {
                            return Json(new { ResponseStatus = -99, ErorrMess = "Thông tin không hợp lệ." });
                        }
                        var reruls = AccountApi.UpdateMobile(currentPhone, currentOtp, newPhone, newOtp, flow);
                        if (reruls == null)
                        {
                            FormsAuthentication.SignOut();
                            SessionsManager.RemoveAllCookie();
                            Session.Abandon();
                            return Json(new { ResponseStatus = -77, ErorrMess = "Hết phiên làm việc" });
                        }
                        return Json(new { ResponseStatus = reruls.responseCode, ErorrMess = reruls.description });
                    }
                }
            }
            return Json(new { ResponseStatus = ResponseStatus, ErorrMess = ErorrMess });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult VerifyPhone()
        {
            int ResponseStatus = -100;
            string ErorrMess = "Hệ thống đang bận, bạn vui lòng quay lại sau";

            if (LoginConfig.AccountId > 0)
            {
                string currentPhone = Request["currentPhone"];
                string otp = Request["otp"];
                string flow = Request["flow"];

                if (!string.IsNullOrEmpty(flow))
                {
                    if (flow == "1")//call bước 1
                    {
                        bool isInvalid = string.IsNullOrEmpty(currentPhone);
                        if (isInvalid)
                        {
                            return Json(new { ResponseStatus = -99, ErorrMess = "Thông tin không hợp lệ." });
                        }

                        var reruls = AccountApi.VerifyMobile(currentPhone, "", "");
                        if (reruls == null)
                        {
                            FormsAuthentication.SignOut();
                            SessionsManager.RemoveAllCookie();
                            Session.Abandon();
                            return Json(new { ResponseStatus = -77, ErorrMess = "Hết phiên làm việc" });
                        }
                        var code = int.Parse(reruls.responseCode);
                        if (code > 0)
                        {
                            return Json(new { ResponseStatus = 1, ErorrMess = "Bước 1 thành công", sign = reruls.extend });
                        }
                        return Json(new { ResponseStatus = reruls.responseCode, ErorrMess = reruls.description });
                    }
                    else//call bước 2
                    {
                        bool isInvalid = string.IsNullOrEmpty(currentPhone) || string.IsNullOrEmpty(otp);
                        if (isInvalid)
                        {
                            return Json(new { ResponseStatus = -99, ErorrMess = "Thông tin không hợp lệ." });
                        }
                        var reruls = AccountApi.VerifyMobile(currentPhone, otp, flow);
                        if (reruls == null)
                        {
                            FormsAuthentication.SignOut();
                            SessionsManager.RemoveAllCookie();
                            Session.Abandon();
                            return Json(new { ResponseStatus = -77, ErorrMess = "Hết phiên làm việc" });
                        }
                        var code = int.Parse(reruls.responseCode);
                        if (code > 0)
                        {
                            return Json(new { ResponseStatus = 1, ErorrMess = "Bước 2 thành công" });
                        }
                        return Json(new { ResponseStatus = reruls.responseCode, ErorrMess = reruls.description });
                    }
                }
            }
            return Json(new { ResponseStatus = ResponseStatus, ErorrMess = ErorrMess });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UnVerifyPhone()
        {
            int ResponseStatus = -100;
            string ErorrMess = "Hệ thống đang bận, bạn vui lòng quay lại sau";

            if (LoginConfig.AccountId > 0)
            {
                string currentPhone = Request["currentPhone"];
                string otp = Request["otp"];

                bool isInvalid = string.IsNullOrEmpty(otp) || string.IsNullOrEmpty(currentPhone);
                if (isInvalid)
                {
                    return Json(new { ResponseStatus = -99, ErorrMess = "Thông tin không hợp lệ." });
                }

                var reruls = AccountApi.UnVerifyMobile(currentPhone, otp);
                if (reruls == null)
                {
                    FormsAuthentication.SignOut();
                    SessionsManager.RemoveAllCookie();
                    Session.Abandon();
                    return Json(new { ResponseStatus = -77, ErorrMess = "Hết phiên làm việc" });
                }
                var code = int.Parse(reruls.responseCode);
                if (code > 0)
                {
                    ViewData.Add("isupdateok", true);//Thêm trạng thái để sau khi load lại trang sẽ về trang info
                    return Json(new { ResponseStatus = 1, ErorrMess = "Thay đổi câu hỏi bảo mật thành công" });
                }
                return Json(new { ResponseStatus = reruls.responseCode, ErorrMess = reruls.description });
            }
            return Json(new { ResponseStatus = ResponseStatus, ErorrMess = ErorrMess });
        }
        #endregion

        #region Thay đổi email
        public PartialViewResult PopupChangeEmail(string accuntName)
        {
            ViewBag.AccountName = accuntName;
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ChangeEmail()
        {
            int ResponseStatus = -100;
            string ErorrMess = "Hệ thống đang bận, bạn vui lòng quay lại sau";

            if (LoginConfig.AccountId > 0)
            {
                string questions = Request["questions"];
                string answer = Request["answer"];
                string oldemail = Request["oldemail"];
                string newemail = Request["newemail"];

                bool isInvalid = string.IsNullOrEmpty(questions) || string.IsNullOrEmpty(answer) || string.IsNullOrEmpty(oldemail) || string.IsNullOrEmpty(newemail);
                if (isInvalid)
                {
                    return Json(new { ResponseStatus = -99, ErorrMess = "Thông tin không hợp lệ." });
                }

                var reruls = AccountApi.UpdateEmail(oldemail, newemail, answer, int.Parse(questions), "");
                if (reruls == null)
                {
                    FormsAuthentication.SignOut();
                    SessionsManager.RemoveAllCookie();
                    Session.Abandon();
                    return Json(new { ResponseStatus = -77, ErorrMess = "Hết phiên làm việc" });
                }
                var codeId = int.Parse(reruls.responseCode);
                if (codeId >= 1)
                {
                    var jsonObject = new UpdateEmailJson()
                    {
                        Questions = questions,
                        Answer = answer,
                        Oldemail = oldemail,
                        NewEmail = newemail,
                    };
                    AbstractDAOFactory.Instance().CreateAccountInfoTempDAO().Insert(reruls.extend, JsonConvert.SerializeObject(jsonObject));
                    var keyBase64 = Security.Base64Encode(reruls.extend);
                    try
                    {
                        using (var mailService = new EBankMailService.MailAPISoapClient())
                        {
                            var pra = $"@Username={LoginConfig.AccountName};@Link={DBCommon.UrlRoot}/verifyemailchange/{keyBase64}";
                            //NLogLogger.LogInfo(pra + " mail Id: 143");
                            var result = mailService.SendMaileBankPaygate(newemail, LoginConfig.AccountName, LoginConfig.AccountId, 143, pra);
                            NLogLogger.Info(string.Format("SendMail Chang Email result:{0}, newemail:{1}, AccountName: {2}", result, newemail, LoginConfig.AccountName));
                        }
                    }
                    catch (Exception ex)
                    {
                        NLogLogger.Info(ex.ToString());
                        NLogLogger.Info(string.Format("SendMail Chang Email result:{0}, newemail:{1}, AccountName: {2}", 0, newemail, LoginConfig.AccountName));
                    }
                }
                return Json(new { ResponseStatus = reruls.responseCode, ErorrMess = reruls.description });
            }
            return Json(new { ResponseStatus = ResponseStatus, ErorrMess = ErorrMess });
        }

        /// <summary>
        /// Xác nhận thay đổi email
        /// </summary>
        /// <returns></returns>
        public ActionResult VerifyEmailChange(string key)
        {
            //chưa đăng nhập thì yêu cầu đăng nhập rồi trả về trang xác nhận
            if (LoginConfig.AccountId <= 0)
            {
                return Redirect($"{DBCommon.UrlRoot}?returnUrl={DBCommon.UrlRoot}verifyemailchange/{key}");
            }

            if (!string.IsNullOrEmpty(key))
            {
                var dbKey = Security.Base64Decode(key);
                var accountInfoTemp = AbstractDAOFactory.Instance().CreateAccountInfoTempDAO().GetById(dbKey);
                if (accountInfoTemp != null && accountInfoTemp.CreatedTime < DateTime.Now.AddDays(7) && !accountInfoTemp.Used)//giới hạn trong vòn 1 tuần
                {
                    var infoJsonPaser = JsonConvert.DeserializeObject<UpdateEmailJson>(accountInfoTemp.AccountInfo);
                    var reruls = AccountApi.UpdateEmail(infoJsonPaser.Oldemail, infoJsonPaser.NewEmail, infoJsonPaser.Answer, int.Parse(infoJsonPaser.Questions), accountInfoTemp.Sign);

                    if (reruls != null)
                    {
                        AbstractDAOFactory.Instance().CreateAccountInfoTempDAO().SetUse(dbKey);//xác nhận đã xử dụng link
                        if (reruls.responseCode == "2")
                        {
                            return Redirect(DBCommon.UrlRoot + "thong-tin-tai-khoan?activeinfo=1");
                        }
                        return Redirect(DBCommon.UrlRoot + "thong-tin-tai-khoan?activeinfo=0");
                    }
                    else
                    {
                        FormsAuthentication.SignOut();
                        SessionsManager.RemoveAllCookie();
                        Session.Abandon();
                        return Redirect($"{DBCommon.UrlRoot}?returnUrl={DBCommon.UrlRoot}verifyemailchange/{key}");
                    }
                }
            }
            return Redirect(DBCommon.UrlRoot);//trở ra trang chủ nếu không có key
        }
        #endregion

        #region Thay đổi câu hỏi bảo mật
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ChangeQuestionByPhone()
        {
            int ResponseStatus = -100;
            string ErorrMess = "Hệ thống đang bận, bạn vui lòng quay lại sau";

            if (LoginConfig.AccountId > 0)
            {
                string currentPhone = Request["currentPhone"];
                string otp = Request["otp"];
                string otptype = Request["otptype"];
                string questionId = Request["questionId"];
                string anwser = Request["anwser"];

                bool isInvalid = string.IsNullOrEmpty(currentPhone) || string.IsNullOrEmpty(otp) || string.IsNullOrEmpty(questionId) || string.IsNullOrEmpty(anwser);
                if (isInvalid)
                {
                    return Json(new { ResponseStatus = -99, ErorrMess = "Thông tin không hợp lệ." });
                }

                var reruls = AccountApi.UpdateQuestion("", 0, anwser, int.Parse(questionId), "", currentPhone, "", "", otp, 1);
                if (reruls == null)
                {
                    FormsAuthentication.SignOut();
                    SessionsManager.RemoveAllCookie();
                    Session.Abandon();
                    return Json(new { ResponseStatus = -77, ErorrMess = "Hết phiên làm việc" });
                }
                var code = int.Parse(reruls.responseCode);
                if (code > 0)
                {
                    return Json(new { ResponseStatus = reruls.responseCode, ErorrMess = "Thay đổi câu hỏi bảo mật thành công" });
                }
                return Json(new { ResponseStatus = reruls.responseCode, ErorrMess = reruls.description });
            }
            return Json(new { ResponseStatus = ResponseStatus, ErorrMess = ErorrMess });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ChangeQuestionByMailB1()
        {
            int ResponseStatus = -100;
            string ErorrMess = "Hệ thống đang bận, bạn vui lòng quay lại sau";

            if (LoginConfig.AccountId > 0)
            {
                string passport = Request["passport"];
                string email = Request["email"];
                string questionId = Request["questionId"];
                string anwser = Request["anwser"];

                bool isInvalid = string.IsNullOrEmpty(passport) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(questionId) || string.IsNullOrEmpty(anwser);
                if (isInvalid)
                {
                    return Json(new { ResponseStatus = -99, ErorrMess = "Thông tin không hợp lệ." });
                }

                var reruls = AccountApi.UpdateQuestion(anwser, int.Parse(questionId), "", 0, email, "", passport, "", "", 0);
                if (reruls == null)
                {
                    FormsAuthentication.SignOut();
                    SessionsManager.RemoveAllCookie();
                    Session.Abandon();
                    return Json(new { ResponseStatus = -77, ErorrMess = "Hết phiên làm việc" });
                }
                var codeId = int.Parse(reruls.responseCode);
                if (codeId >= 1)
                {
                    var jsonObject = new UpdateQuestionByEmailJson()
                    {
                        Answer = anwser,
                        Passport = passport,
                        Email = email,
                        QuestionsId = questionId
                    };
                    AbstractDAOFactory.Instance().CreateAccountInfoTempDAO().Insert(reruls.extend, JsonConvert.SerializeObject(jsonObject));
                    var keyBase64 = Security.Base64Encode(reruls.extend);
                    try
                    {
                        using (var mailService = new EBankMailService.MailAPISoapClient())
                        {
                            var pra = $"@Username={LoginConfig.AccountName};@Link={DBCommon.UrlRoot}/doi-cau-hoi/{keyBase64}";
                            //NLogLogger.LogInfo(pra + "Mail Id : 1");
                            var result = mailService.SendMaileBankPaygate(email, LoginConfig.AccountName, LoginConfig.AccountId, 7549, pra);
                            NLogLogger.Info(string.Format("SendMail UpdateQuestion Email result:{0}, newemail:{1}, AccountName: {2}", result, email, LoginConfig.AccountName));
                        }
                    }
                    catch (Exception ex)
                    {
                        NLogLogger.Info(ex.ToString());
                    }
                }
                return Json(new { ResponseStatus = reruls.responseCode, ErorrMess = reruls.description });
            }
            return Json(new { ResponseStatus = ResponseStatus, ErorrMess = ErorrMess });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ChangeQuestionByMailB2()
        {
            int ResponseStatus = -100;
            string ErorrMess = "Hết phiên làm việc, cần đăng nhập lại";

            if (LoginConfig.AccountId > 0)
            {
                string newquestionsid = Request["newquestionsid"];
                string newanswer = Request["newanswer"];
                string key = Request["key"];

                bool isInvalid = string.IsNullOrEmpty(key) || string.IsNullOrEmpty(newanswer) || string.IsNullOrEmpty(newquestionsid);
                if (isInvalid)
                {
                    return Json(new { ResponseStatus = -99, ErorrMess = "Thông tin thiếu, không hợp lệ." });
                }

                var dbKey = Security.Base64Decode(key);
                var accountInfoTemp = AbstractDAOFactory.Instance().CreateAccountInfoTempDAO().GetById(dbKey);
                if (accountInfoTemp != null && accountInfoTemp.CreatedTime < DateTime.Now.AddDays(7) && !accountInfoTemp.Used)//giới hạn trong vòn 1 tuần
                {
                    var infoJsonPaser = JsonConvert.DeserializeObject<UpdateQuestionByEmailJson>(accountInfoTemp.AccountInfo);
                    var reruls = AccountApi.UpdateQuestion(infoJsonPaser.Answer, int.Parse(infoJsonPaser.QuestionsId), newanswer, int.Parse(newquestionsid), infoJsonPaser.Email, "", infoJsonPaser.Passport, dbKey, "", 0);
                    if (reruls == null)
                    {
                        FormsAuthentication.SignOut();
                        SessionsManager.RemoveAllCookie();
                        Session.Abandon();
                        return Json(new { ResponseStatus = -77, ErorrMess = "Hết phiên làm việc" });
                    }
                    else
                    {
                        AbstractDAOFactory.Instance().CreateAccountInfoTempDAO().SetUse(dbKey);//xác nhận đã xử dụng link
                        if (int.Parse(reruls.responseCode) >= 1)
                        {
                            return Json(new { ResponseStatus = 1, ErorrMess = "Thanh đổi cầu hỏi bảo mật thành công" });
                        }
                        return Json(new { ResponseStatus = int.Parse(reruls.responseCode), ErorrMess = reruls.description });
                    }
                }
                else
                {
                    return Json(new { ResponseStatus = -98, ErorrMess = "Hết hạn kích hoạt" });
                }
            }
            return Json(new { ResponseStatus = ResponseStatus, ErorrMess = ErorrMess });
        }
        #endregion

        #region Chuyên khoản
        public PartialViewResult PopupChuyenKhoan()
        {
            var balance = AccountApi.GetBalance();
            if (balance == null)
            {
                FormsAuthentication.SignOut();
                SessionsManager.RemoveAllCookie();
                Session.Abandon();
                Response.Redirect(Config.UrlRoot);
            }
            ViewBag.Balance = balance.vcoin;
            return PartialView();
        }
        #endregion

        #region Thông tin khách hàng
        public PartialViewResult TabVip(string viptab = "0", int itemId = 0)
        {
            AccountNew model = new AccountNew();
            var service = AbstractDAOFactory.Instance().CreateNewGiftShopDAO();
            var responValue = 0;
            var account = service.WEB__Accounts_GetInfor(LoginConfig.AccountId, out responValue);
            if (account != null && responValue > 0)
            {
                model = account;
                if (model.CustomerID > 0)
                {
                    responValue = -1;
                    var customer = service.WEB__Customer_GetInfor(model.CustomerID, out responValue);
                    if (customer != null && responValue > 0)
                    {
                        if (string.IsNullOrWhiteSpace(customer.Mobile))
                        {
                            var accDetail = AccountApi.GetAccountDetail();
                            if (accDetail != null && !string.IsNullOrWhiteSpace(accDetail.mobile))
                            {
                                customer.Mobile = accDetail.mobile;
                                customer.CustomerName = string.IsNullOrWhiteSpace(accDetail.fullName) ? "" : accDetail.fullName;
                                customer.Email = accDetail.email;
                                customer.Birthday = accDetail.birthdayDate;
                                customer.District = accDetail.districtId;
                                customer.Province = accDetail.locationId;
                                customer.Ward = accDetail.wardId;
                                customer.Address = accDetail.address;
                                customer.Gender = (byte)accDetail.gender;
                                long updateCheck = 0;
                                service.WEB__Customer_Update(customer, out updateCheck);
                            }
                        }
                        model.Customer = customer;
                    }
                }
                if (model.Customer == null)
                {
                    model.Customer = new Customer() { National = 1 };
                }
            }

            model.Customer.National = 1;
            ViewBag.Viptab = viptab;
            ViewBag.ItemId = itemId;

            //Check đến trường hợp truy cập là mobile thì chuyển sang trang mobile
            if (IsMobileVersion)
                return PartialView("WapTabVip", model);

            return PartialView(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SaveCustomerInfo()
        {
            int ResponseStatus = -100;
            string ErorrMess = "Hệ thống đang bận, bạn vui lòng quay lại sau";
            var responValue = 0;
            var account = AbstractDAOFactory.Instance().CreateNewGiftShopDAO().WEB__Accounts_GetInfor(LoginConfig.AccountId, out responValue);
            if (account != null)
            {
                string customerName = Request["customerName"];
                string customerMobile = Request["customerMobile"];
                byte customerGender = Convert.ToByte(Request["customerGender"]);
                int dfbday = Convert.ToInt32(Request["dfbday"]);
                int dfbmonth = Convert.ToInt32(Request["dfbmonth"]);
                int dfbyear = Convert.ToInt32(Request["dfbyear"]);
                byte national = Convert.ToByte(Request["national"]);
                int cbcityId = Convert.ToInt32(Request["cbcityId"]);
                int cbdistrictId = Convert.ToInt32(Request["cbdistrictId"]);
                int cbwardId = Convert.ToInt32(Request["cbwardId"]);
                string address = Request["address"];
                string facebookLink = Request["facebookLink"];
                byte works = Convert.ToByte(Request["works"]);
                byte marriage = Convert.ToByte(Request["marriage"]);

                if (string.IsNullOrEmpty(customerName) || string.IsNullOrEmpty(customerMobile) || string.IsNullOrEmpty(customerName))
                {
                    return Json(new { ResponseStatus = -4, ErorrMess = "Sai dữ liệu đầu vào" });
                }

                if (StringUtility.CheckScriptTag(facebookLink) || StringUtility.CheckScriptTag(address) || !StringUtility.CheckValidMobile(customerMobile))
                {
                    return Json(new { ResponseStatus = -4, ErorrMess = "Dữ liệu đầu vào không hợp lệ" });
                }

                Customer customer;
                if (account.CustomerID > 0)
                {
                    customer = AbstractDAOFactory.Instance().CreateNewGiftShopDAO().WEB__Customer_GetInfor(account.CustomerID, out responValue);
                    customer.CustomerName = customerName;
                    customer.Mobile = customerMobile;
                    customer.Gender = customerGender;
                    customer.Birthday = new DateTime(dfbyear, dfbmonth, dfbday);
                    customer.National = national;
                    customer.Province = cbcityId;
                    customer.District = cbdistrictId;
                    customer.Ward = cbwardId;
                    customer.Address = address;
                    customer.FacebookLink = facebookLink;
                    customer.Works = works;
                    customer.Marriage = marriage;
                    long updateStatus = 0;
                    AbstractDAOFactory.Instance().CreateNewGiftShopDAO().WEB__Customer_Update(customer, out updateStatus);
                    if (updateStatus > 0)
                    {
                        return Json(new { ResponseStatus = 1, ErorrMess = "Sửa thông tin khách hàng thành công." });
                    }
                }
                else
                {
                    customer = new Customer();
                    customer.CustomerName = customerName;
                    customer.Mobile = customerMobile;
                    customer.Gender = customerGender;
                    customer.Birthday = new DateTime(dfbyear, dfbmonth, dfbday);
                    customer.National = national;
                    customer.Province = cbcityId;
                    customer.District = cbdistrictId;
                    customer.Ward = cbwardId;
                    customer.Address = address;
                    customer.FacebookLink = facebookLink;
                    customer.Works = works;
                    customer.Marriage = marriage;
                    int updateStatus = 0;
                    AbstractDAOFactory.Instance().CreateNewGiftShopDAO().WEB__Customer_AddNew(customer, account.AccountID, out updateStatus);
                    if (updateStatus > 0)
                    {
                        return Json(new { ResponseStatus = 1, ErorrMess = "Thêm thông tin khách hàng thành công." });
                    }
                }
            }
            return Json(new { ResponseStatus = ResponseStatus, ErorrMess = ErorrMess });
        }

        public JsonResult DiemTichLuy180Ngay(long customerId, int page = 1, int pageSize = 10)
        {
            var totalRow = 0;
            var listDiemTichLuy = AbstractDAOFactory.Instance().CreateNewGiftShopDAO().WEB__Customer_HistoryChangeScoreVIP(customerId, page, pageSize, out totalRow);
            return Json(new
            {
                List = listDiemTichLuy != null ?
                listDiemTichLuy.Select(q => new
                {
                    Time = q.TransDate.ToString("dd-MM-yyyy HH:mm:ss"),
                    Score = q.Score,
                    ServiceName = q.Description
                })
                : null,
                TotalRow = 0
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LichSuThayDoi(long customerId, string fromDate = "", string todate = "", int page = 1, int pageSize = 10)
        {
            var totalRow = 0;
            DateTime? pformdate = null;
            DateTime? ptodate = null;
            if (!string.IsNullOrEmpty(fromDate))
            {
                pformdate = DateTime.ParseExact(fromDate, "dd/MM/yyyy", null);
            }
            if (!string.IsNullOrEmpty(todate))
            {
                ptodate = DateTime.ParseExact(todate, "dd/MM/yyyy", null);
            }
            var listDiemTichLuy = AbstractDAOFactory.Instance().CreateNewGiftShopDAO().WEB__Customer_HistoryChangeScoreSwap(customerId, pformdate, ptodate, page, pageSize, out totalRow);
            return Json(new
            {
                List = listDiemTichLuy != null ?
                listDiemTichLuy.Select(q => new
                {
                    Time = q.TransDate.ToString("dd-MM-yyyy HH:mm:ss"),
                    Score = q.Score,
                    ServiceName = q.Description,
                    q.Dir,
                })
                : null,
                TotalRow = 0
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Box thông tin khách hàng dùng chung
        /// </summary>
        /// <returns>PartialViewResult</returns>
        public PartialViewResult UserInfo()
        {
            var accountId = LoginConfig.AccountId;
            var responValue = 0;
            var account = AbstractDAOFactory.Instance().CreateNewGiftShopDAO().WEB__Accounts_GetInfor(accountId, out responValue);
            if (responValue > 0 && account != null && account.CustomerID > 0)
            {
                account.Customer = AbstractDAOFactory.Instance().CreateNewGiftShopDAO().WEB__Customer_GetInfor(account.CustomerID, out responValue);
            }
            return PartialView(account);
        }
        #endregion

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateInfo()
        {
            int ResponseStatus = -100;
            string ErorrMess = "lỗi mặc định";
            //cần validate input
            string FullName = Request["FullName"];
            string Phone = Request["Phone"];
            int Gender = int.Parse(Request["Gender"]);
            string Day = Request["Day"];
            string Month = Request["Month"];
            string Year = Request["Year"];
            string Place = Request["Place"];
            if (Day.Length < 2)
                Day = "0" + Day;
            if (Month.Length < 2)
                Month = "0" + Month;
            string dateTimeString = Day + "-" + Month + "-" + Year;
            string inputFormat = "dd-MM-yyyy";
            var birthday = DateTime.ParseExact(dateTimeString, inputFormat, System.Globalization.CultureInfo.InvariantCulture);
            //long birthday_Milisecond = dateTime.Ticks / TimeSpan.TicksPerMillisecond;

            if (LoginConfig.AccountId > 0)
            {
                #region[Update Thông Tin]
                string URL = ConfigurationManager.AppSettings["API"];
                URL += "update/profile";
                string[] ParamName = new string[] { "account_id", "address", "birthday", "client_ip", "full_name", "gender", "mobile", "location_id", "ward_id", "district_id", "access_token" };
                string[] ParamValue = new string[11];


                ParamValue[0] = LoginConfig.AccountId.ToString();// account_id
                ParamValue[1] = Place;//address
                ParamValue[2] = DateUtilities.ConvertDateTimeToTimeStamp(birthday).ToString();//address
                ParamValue[3] = DBCommon.ClientIP;
                ParamValue[4] = FullName;
                ParamValue[5] = Gender.ToString(); //0 là nam, 1 là nữ;
                ParamValue[6] = Phone;
                ParamValue[7] = "1";
                ParamValue[8] = "1";
                ParamValue[9] = "1";
                ParamValue[10] = LoginConfig.AccessToken;
                string OPutUpdateInfo = new API().PostURL(URL, ParamName, ParamValue);
                if (!string.IsNullOrEmpty(OPutUpdateInfo))
                {
                    InfoUpdate Info = Newtonsoft.Json.JsonConvert.DeserializeObject<InfoUpdate>(OPutUpdateInfo);
                    if (!string.IsNullOrEmpty(Info.responseCode))
                    {
                        ResponseStatus = int.Parse(Info.responseCode);
                        ErorrMess = Info.description;
                    }

                }
                #endregion
            }

            return Json(new { ResponseStatus = ResponseStatus, ErorrMess = ErorrMess });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SetReadNoti(int id)
        {
            var responseStatus = -99;
            var responseMessage = "Hệ thống đang bận hoặc bảo trì.";

            if (LoginConfig.AccountId > 0)
            {
                responseStatus = AbstractDAOFactory.Instance().CreateMessageNoticeDAO().NotifiSetRead(id, LoginConfig.AccountId);
                if (responseStatus == 1)
                {
                    responseMessage = "Thành công.";
                }
                else
                {
                    responseMessage = "Không tìm thấy tin nhắn.";
                }
            }

            return Json(new { ResponseStatus = responseStatus, ErorrMess = responseMessage });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetBalanceAll()
        {
            int Vcoin = -1;
            int VcoinFreeze = -1;
            if (LoginConfig.AccountId > 0)
            {

                Balance Info = AccountApi.GetBalance();
                if (Info != null)
                {
                    Vcoin = int.Parse(Info.vcoin);
                    VcoinFreeze = int.Parse(Info.vcoinFreeze);
                }
                return Json(new { Vcoin = Vcoin, VcoinFreeze = VcoinFreeze });

            }
            else
            {
                return Json(new { Vcoin = 0, VcoinFreeze = 0 });
            }

        }

        #region "Chuyển khoản"
        [HttpPost]
        public JsonResult CheckAcount(string acountName)
        {
            int ResponseStatus = -100;
            string ErorrMess = "";
            try
            {
                if (!string.IsNullOrEmpty(acountName))
                {

                    var keyCheckAccountName = "checkaccoutname";
                    if (Session[keyCheckAccountName] == null)
                    {
                        Session[keyCheckAccountName] = "1";
                    }
                    if (Convert.ToInt32(Session[keyCheckAccountName]) <= 50)
                    {
                        var number = Convert.ToInt32(Session[keyCheckAccountName]) + 1;
                        Session[keyCheckAccountName] = number.ToString();


                        var data = WebApiApplication.accounService.CheckVTCAccount(acountName);
                        if (data.ResponseCode > 0)
                        {
                            ResponseStatus = 99;
                            ErorrMess = "C";
                        }
                        else
                        {
                            ResponseStatus = -50;
                            ErorrMess = "Tài khoản chưa tồn tại";
                        }

                    }
                    else
                    {
                        ResponseStatus = -86;
                        ErorrMess = "Số lần kiểm qua quá nhiều.";
                    }


                }
                else
                {
                    ResponseStatus = -88;
                    ErorrMess = "Bạn chưa nhập tên tài khoản.";
                }
            }
            catch (Exception ex)
            {
                NLogLogger.PublishException(ex);
            }
            return Json(new { ResponseStatus = ResponseStatus, ErorrMess = ErorrMess });
        }
        [HttpPost]
        public JsonResult GetTransferVcoinPolicy(int amount)
        {
            int VcoinFee = -1;
            int VcoinRecive = -1;
            if (LoginConfig.AccountId > 0)
            {

                TransferPolicyResponseData Info = AccountApi.GetTransferVcoinPolicy(amount);
                if (Info != null)
                {
                    VcoinFee = Info.vcoinFee;
                    VcoinRecive = Info.vcoinRecive;
                }
                return Json(new { VcoinFee = VcoinFee, VcoinRecive = VcoinRecive });

            }
            else
            {
                return Json(new { VcoinFee = 0, VcoinRecive = 0 });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Tranfer()
        {
            int ResponseStatus = -100;
            string ErorrMess = "Hệ thống đang bận, bạn vui lòng quay lại sau";

            if (LoginConfig.AccountId > 0)
            {
                string ReciveAccountName = Request["ReciveAccountName"];
                int VcoinValue = 0;
                int.TryParse(Request["VcoinValue"].ToString(), out VcoinValue);
                string sign = "";
                string otp = "";
                string note = Request["Note"];
                string flow = Request["flow"];
                long transId = 0;
                long clientTime = 0;
                if (!string.IsNullOrEmpty(flow))
                {
                    if (flow == "1")//call bước 1
                    {
                        bool isInvalid = string.IsNullOrEmpty(ReciveAccountName) || string.IsNullOrEmpty(note) || VcoinValue < 10;
                        if (isInvalid)
                        {
                            return Json(new { ResponseStatus = -99, ErorrMess = "Thông tin không hợp lệ." });
                        }
                        string captcha = Request["captcha"];
                        string captchaVerify = Request["VeryfyCaptcha"];
                        //var verifyCaptcha = new CapChaController.CaptchaController().VerifyCaptcha(captcha, captchaVerify);

                        if (!captcha.Equals(Session["Captcha"].ToString()))
                        {
                            return Json(new { ResponseStatus = -55, ErorrMess = "Sai mã kiểm tra." });
                        }
                        transId = long.Parse(DateTime.Now.ToString("yyMMddHHmmss"));
                        clientTime = DateUtilities.ConvertDateTimeToTimeStamp(DateTime.Now);
                        var reruls = AccountApi.Transfer(VcoinValue, ReciveAccountName, transId, clientTime, otp, note, sign);
                        if (reruls == null)
                        {
                            FormsAuthentication.SignOut();
                            SessionsManager.RemoveAllCookie();
                            Session.Abandon();
                            return Json(new { ResponseStatus = -77, ErorrMess = "Hết phiên làm việc" });
                        }
                        if (reruls.responseCode == -2002)
                            reruls.responseCode = 1;
                        return Json(new { ResponseStatus = reruls.responseCode, ErorrMess = reruls.description, transId = transId, clientTime = clientTime });
                    }
                    else//call bước 2
                    {
                        long.TryParse(Request["transId"].ToString(), out transId);
                        long.TryParse(Request["clientTime"].ToString(), out clientTime);
                        //sign = Request["sign"];
                        otp = Request["otp"];
                        bool isInvalid = string.IsNullOrEmpty(ReciveAccountName) || string.IsNullOrEmpty(otp) || string.IsNullOrEmpty(note) || VcoinValue < 10 || transId < 0 || clientTime < 0;
                        if (isInvalid)
                        {
                            return Json(new { ResponseStatus = -99, ErorrMess = "Thông tin không hợp lệ." });
                        }
                        var reruls = AccountApi.Transfer(VcoinValue, ReciveAccountName, transId, clientTime, otp, note, sign);
                        if (reruls == null)
                        {
                            FormsAuthentication.SignOut();
                            SessionsManager.RemoveAllCookie();
                            Session.Abandon();
                            return Json(new { ResponseStatus = -77, ErorrMess = "Hết phiên làm việc" });
                        }
                        return Json(new { ResponseStatus = reruls.responseCode, ErorrMess = reruls.description });
                    }
                }
            }
            return Json(new { ResponseStatus = ResponseStatus, ErorrMess = ErorrMess });
        }
        #endregion

        #region "bảo mật"




        /// <summary>
        /// Update SMS Plus
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateSMSPlus()
        {
            int ResponseStatus = -100;
            string ErorrMess = "lỗi mặc định";

            int DV_TruyVanTT = Convert.ToInt32(bool.Parse(Request["DV_TruyVanTT"]));
            int NhanTN_QuangCaoFree = Convert.ToInt32(bool.Parse(Request["NhanTN_QuangCaoFree"]));
            int vcoinMinMax = 0;
            int.TryParse(Request["vcoinMinMax"], out vcoinMinMax);
            int TN_ChuDong = Convert.ToInt32(bool.Parse(Request["TN_ChuDong"]));
            int VcoinTang = Convert.ToInt32(bool.Parse(Request["VcoinTang"]));
            int VcoinGiam = Convert.ToInt32(bool.Parse(Request["VcoinGiam"]));
            //int BaoMatOTP = Convert.ToInt32(bool.Parse(Request["BaoMatOTP"]));
            //int BaoMatODP = Convert.ToInt32(bool.Parse(Request["BaoMatODP"]));

            if (LoginConfig.AccountId > 0)
            {
                var apiInfo = AccountApi.UpdateSMSPlus(NhanTN_QuangCaoFree, TN_ChuDong, VcoinGiam, VcoinTang, vcoinMinMax, DV_TruyVanTT);
                if (apiInfo == null)
                {
                    FormsAuthentication.SignOut();
                    SessionsManager.RemoveAllCookie();
                    Session.Abandon();
                    return Json(new { ResponseStatus = -77, ErorrMess = "Hết phiên làm việc" });
                }
                ResponseStatus = int.Parse(apiInfo.responseCode);
                ErorrMess = apiInfo.description;


            }
            else
            {
                ResponseStatus = -101;
                ErorrMess = "Bạn chưa đăng nhập";
            }


            return Json(new { ResponseStatus = ResponseStatus, ErorrMess = ErorrMess });
        }
        /// <summary>
        /// Setup đăng nhập bảo mật
        /// </summary>
        /// <param name="setupType"></param>
        /// <param name="secureCode"></param>
        /// <param name="secureType"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AuthenSecure(int setupType, string secureCode, int secureType)
        {


            int ResponseStatus = -100;
            string ErorrMess = "Hệ thống đang bận, bạn vui lòng quay lại sau";

            if (LoginConfig.AccountId > 0)
            {
                //string captcha = Request["captchaChangePass"];
                //string captchaVerify = Request["VeryfyChangePass"];
                //var verifyCaptcha = new VTCGame.Portal.Controllers.CapChaController.CaptchaController().VerifyCaptcha(captcha, captchaVerify);

                //if (Convert.ToInt32(verifyCaptcha) <= 0)
                //{
                //    return Json(new { ResponseStatus = -55, ErorrMess = "Sai mã kiểm tra." });
                //}

                var apiInfo = AccountApi.AuthenSecure(secureType, secureCode, setupType);
                if (apiInfo == null)
                {
                    FormsAuthentication.SignOut();
                    SessionsManager.RemoveAllCookie();
                    Session.Abandon();
                    return Json(new { ResponseStatus = -77, ErorrMess = "Hết phiên làm việc" });
                }
                ResponseStatus = int.Parse(apiInfo.responseCode);
                ErorrMess = apiInfo.description;


            }
            else
            {
                ResponseStatus = -101;
                ErorrMess = "Bạn chưa đăng nhập";
            }
            return Json(new { ResponseStatus = ResponseStatus, ErorrMess = ErorrMess });
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ChangePass()
        {
            int ResponseStatus = -100;
            string ErorrMess = "Hệ thống đang bận, bạn vui lòng quay lại sau";

            string PassOld = Request["PassOld"];
            string PassNew = Request["PassNew"];
            string RePassNew = Request["RePassNew"];
            string MaOtp = Request["MaOtp"];
            int OtpType = int.Parse(Request["OtpType"]);

            if (LoginConfig.AccountId > 0)
            {
                string captcha = Request["captchaChangePass"];
                string captchaVerify = Request["VeryfyChangePass"];
                //var verifyCaptcha = new VTCGame.Portal.Controllers.CapChaController.CaptchaController().VerifyCaptcha(captcha, captchaVerify);

                if (!captcha.Equals(Session["Captcha"].ToString()))
                {
                    return Json(new { ResponseStatus = -55, ErorrMess = "Sai mã kiểm tra." });
                }
                var apiInfo = AccountApi.UpdatePassword(PassOld, PassNew, OtpType, MaOtp);
                if (apiInfo == null)
                {
                    FormsAuthentication.SignOut();
                    SessionsManager.RemoveAllCookie();
                    Session.Abandon();
                    return Json(new { ResponseStatus = -77, ErorrMess = "Hết phiên làm việc" });
                }
                ResponseStatus = int.Parse(apiInfo.responseCode);
                ErorrMess = apiInfo.description;


            }
            else
            {
                ResponseStatus = -101;
                ErorrMess = "Bạn chưa đăng nhập";
            }
            return Json(new { ResponseStatus = ResponseStatus, ErorrMess = ErorrMess });
        }

        /// <summary>
        /// Mở đóng băng Vcoin
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DongMoBang()
        {
            int TypeIce = int.Parse(Request["TypeIce"]); //1 dong bang, 2 mo bang
            int OtpTypeIce = int.Parse(Request["OtpTypeIce"]); //1-OTP SMS, 2-OTP APP
            int so_vcoin = int.Parse(Request["so_vcoin"]);
            string maOTPIce = Request["maOTPIce"];

            int ResponseStatus = -100;
            string ErorrMess = "lỗi mặc định";
            if (TypeIce == 1)//Dong Bang Vcoin
            {
                if (LoginConfig.AccountId > 0)
                {
                    var apiInfo = AccountApi.Freeze(so_vcoin);

                    if (apiInfo == null)
                    {
                        FormsAuthentication.SignOut();
                        SessionsManager.RemoveAllCookie();
                        Session.Abandon();
                        return Json(new { ResponseStatus = -77, ErorrMess = "Hết phiên làm việc" });
                    }
                    ResponseStatus = int.Parse(apiInfo.responseCode);
                    ErorrMess = apiInfo.description;
                }
            }
            else if (TypeIce == 2)//Mo Bang Vcoin
            {
                if (LoginConfig.AccountId > 0)
                {
                    var apiInfo = AccountApi.UnFreeze(so_vcoin, OtpTypeIce, maOTPIce);

                    if (apiInfo == null)
                    {
                        FormsAuthentication.SignOut();
                        SessionsManager.RemoveAllCookie();
                        Session.Abandon();
                        return Json(new { ResponseStatus = -77, ErorrMess = "Hết phiên làm việc" });
                    }
                    ResponseStatus = int.Parse(apiInfo.responseCode);
                    ErorrMess = apiInfo.description;

                }
            }
            else
            {
                ResponseStatus = -101;
                ErorrMess = "Bạn chưa đăng nhập";
            }
            return Json(new { ResponseStatus = ResponseStatus, ErorrMess = ErorrMess });
        }
        /// <summary>
        /// Tab Đổi mật khẩu
        /// </summary>
        /// <returns></returns>
        public PartialViewResult TabChangePassword()
        {
            if (IsMobileVersion)
                return PartialView("Wap_TabChangePassword");
            return PartialView();


        }

        /// <summary>
        /// Tab SMS Plus
        /// </summary>
        /// <returns></returns>
        public PartialViewResult TabSMSPlus()
        {
            //kiểm tra tài khoản đã đăng ký otp chưa
            var info = AccountApi.GetAccountMobile();
            TempData["MobileStatus"] = -1;
            if (info == null)
            {
                FormsAuthentication.SignOut();
                SessionsManager.RemoveAllCookie();
                Session.Abandon();
                Response.Redirect(Config.UrlRoot);
            }
            TempData["MobileStatus"] = info.status;
            TempData["Phone"] = info.mobile;
            var model = new SMSPlusModel();
            if (info.status > 0)
            {
                var infoplus = AccountApi.GetSmsPlusList();
                if (infoplus == null)
                {
                    FormsAuthentication.SignOut();
                    SessionsManager.RemoveAllCookie();
                    Session.Abandon();
                    Response.Redirect(Config.UrlRoot);
                }

                var listService = infoplus.listSmsPlus;

                if (listService.Count > 0)
                {
                    var advObj = listService.Where(x => x.smsserviceID == 2).FirstOrDefault();
                    var smsObj = listService.Where(x => x.smsserviceID == 3).FirstOrDefault();

                    if (advObj != null)
                        model.adv = advObj.serviceStatus > 0 ? true : false;
                    if (smsObj != null)
                    {
                        model.balanceDesc = smsObj.balanceDesc;
                        model.balanceInc = smsObj.balanceInc;
                        model.minAmount = smsObj.minAmount;
                    }

                }

            }
            if (IsMobileVersion)
                return PartialView("Wap_TabSMSPlus", model);
            return PartialView(model);
        }
        /// <summary>
        /// Tab setup đăng nhập bảo mật
        /// </summary>
        /// <returns></returns>
        public PartialViewResult TabSecureAuthen()
        {
            //kiểm tra tài khoản đã đăng ký otp chưa
            var info = AccountApi.GetAccountMobile();
            TempData["MobileStatus"] = -1;
            if (info == null)
            {
                FormsAuthentication.SignOut();
                SessionsManager.RemoveAllCookie();
                Session.Abandon();
                Response.Redirect(Config.UrlRoot);
            }
            TempData["MobileStatus"] = info.status;
            TempData["Phone"] = info.mobile;


            var infoauthen = AccountApi.CheckSecure();
            TempData["AuthenStatus"] = -1;
            if (infoauthen == null)
            {
                FormsAuthentication.SignOut();
                SessionsManager.RemoveAllCookie();
                Session.Abandon();
                Response.Redirect(Config.UrlRoot);
            }
            TempData["AuthenStatus"] = infoauthen.responseCode;

            if (IsMobileVersion)
                return PartialView("Wap_TabSecureAuthen");
            return PartialView();
        }
        /// <summary>
        /// Tab bảo mật
        /// </summary>
        /// <param name="subtype"></param>
        /// <returns></returns>
        public PartialViewResult TabSMS(int subtype = 0)
        {

            ViewBag.subtype = subtype;
            if (IsMobileVersion)
                return PartialView("Wap_TabSMS", subtype);
            return PartialView();

        }
        /// <summary>
        /// Tab đóng băng Vcoin
        /// </summary>
        /// <returns></returns>
        public PartialViewResult TabFreeze()
        {
            //kiểm tra tài khoản đã đăng ký otp chưa
            var info = AccountApi.GetAccountMobile();
            TempData["MobileStatus"] = -1;
            if (info == null)
            {
                FormsAuthentication.SignOut();
                SessionsManager.RemoveAllCookie();
                Session.Abandon();
                Response.Redirect(Config.UrlRoot);
            }
            TempData["MobileStatus"] = info.status;
            TempData["Phone"] = info.mobile;

            if (IsMobileVersion)
                return PartialView("Wap_TabFreeze");
            return PartialView();
        }
        /// <summary>
        /// Game Online
        /// </summary>
        /// <returns></returns>
        public PartialViewResult GameOnline()
        {

            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SetupGameOnline()
        {
            int server = int.Parse(Request["server"]);
            string accountname = Request["accountname"].ToString();
            string password = Request["password"];
            int gender = int.Parse(Request["gender"]);
            int ResponseStatus = 100;
            string ErorrMess = "lỗi mặc định";

            return Json(new { ResponseStatus = ResponseStatus, ErorrMess = ErorrMess });
        }
        public PartialViewResult PopupGameOnline(int Server)
        {
            ViewBag.Server = Server;
            switch (Server)
            {
                case 6:
                    ViewBag.ServerName = "Tạo nhân vật server Hà Nội";
                    break;
                case 7:
                    ViewBag.ServerName = "Tạo nhân vật server TP HCM";
                    break;
                case 8:
                    ViewBag.ServerName = "Tạo nhân vật  server Đà Nẵng";
                    break;
                default:
                    ViewBag.ServerName = "Tạo nhân vật";
                    break;

            }
            return PartialView();
        }
        /// <summary>
        /// Tab đổi câu hỏi bảo mật
        /// </summary>
        /// <returns></returns>
        public PartialViewResult TabQuestion(string key = "")
        {
            //kiểm tra tài khoản đã đăng ký otp chưa
            var info = AccountApi.GetAccountMobile();
            TempData["MobileStatus"] = -1;
            if (info == null)
            {
                FormsAuthentication.SignOut();
                SessionsManager.RemoveAllCookie();
                Session.Abandon();
                Response.Redirect(Config.UrlRoot);
            }
            TempData["MobileStatus"] = info.status;
            TempData["Phone"] = info.mobile;
            ViewBag.Key = key;
            if (IsMobileVersion)
                return PartialView("Wap_TabQuestion", key);
            return PartialView();
        }

        #endregion

        #region "Lịch sử giao dịch"
        public PartialViewResult HistoryTransaction()
        {
            //var dateFrom = DateTime.Now.AddMonths(-1).AddDays(1);
            //var dateTo = DateTime.Now.AddDays(1);
            //var history = AccountApi.GetHistoryTransaction(dateFrom, dateTo);
            if (IsMobileVersion)
                return PartialView("Wap_HistoryTransaction");
            return PartialView();
        }
        public PartialViewResult GetHistoryTransaction(int? currentPage, int? pageSize)
        {
            var dateFrom = DateTime.Now.AddMonths(-1).AddMinutes(1);
            var dateTo = DateTime.Now.AddMinutes(1);
            var history = AccountApi.GetHistoryTransaction(dateFrom, dateTo);

            int TotalRecord = 0;
            int CurrPage = currentPage == null ? 1 : (int)currentPage;
            int RecordPerPage = pageSize == null ? 10 : (int)pageSize;
            if (history != null && history.Count > 0)
            {
                TotalRecord = history.Count;
                ViewBag.TotalRecord = TotalRecord;
                history = history.Skip(RecordPerPage * (CurrPage - 1)).Take(RecordPerPage).ToList();

            }
            else
            {
                ViewBag.TotalRecord = 0;
            }
            int TotalPage = (int)(TotalRecord / RecordPerPage);
            if (TotalRecord > 0)
            {
                if (TotalRecord % RecordPerPage > 0)
                    TotalPage += 1;
            }
            ViewBag.TotalPage = TotalPage;
            ViewBag.CurrentPage = CurrPage;
            ViewBag.pageSize = pageSize;
            return PartialView(history);
        }
        public PartialViewResult GetHistoryTransactionWap(int? currentPage, int? pageSize)
        {
            var dateFrom = DateTime.Now.AddMonths(-1).AddMinutes(1);
            var dateTo = DateTime.Now.AddMinutes(1);
            var history = AccountApi.GetHistoryTransaction(dateFrom, dateTo);

            int TotalRecord = 0;
            int CurrPage = currentPage == null ? 1 : (int)currentPage;
            int RecordPerPage = pageSize == null ? 10 : (int)pageSize;
            if (history != null && history.Count > 0)
            {
                TotalRecord = history.Count;
                ViewBag.TotalRecord = TotalRecord;
                history = history.Skip(RecordPerPage * (CurrPage - 1)).Take(RecordPerPage).ToList();

            }
            else
            {
                ViewBag.TotalRecord = 0;
            }
            int TotalPage = (int)(TotalRecord / RecordPerPage);
            if (TotalRecord > 0)
            {
                if (TotalRecord % RecordPerPage > 0)
                    TotalPage += 1;
            }
            ViewBag.TotalPage = TotalPage;
            ViewBag.CurrentPage = CurrPage;
            ViewBag.pageSize = pageSize;
            return PartialView(history);
        }
        #endregion
        public PartialViewResult AccountMessage()
        {
            var userId = LoginConfig.AccountId;
            var username = LoginConfig.AccountName;
            ReturnUserInfo returnUserInfo = null;
            if (userId > 0)
            {
                var VIPID = 0;
                var VIPName = "";
                var scoreRank = 0;
                var scoreSwap = 0;
                var nextVip = "";
                var nextVipScore = 0;
                var isRegisterVip = 0;
                var avartar = "";
                var address = "";
                returnUserInfo = AbstractDAOFactory.Instance().CreateEventDAO().GetMyInfo(userId, ref username, ref VIPID, ref VIPName, ref scoreRank, ref scoreSwap, ref nextVip, ref nextVipScore, ref isRegisterVip, ref avartar, ref address);
            }

            return PartialView(returnUserInfo);
        }

        public JsonResult BaoDanh()
        {
            string GiftName = "";
            int GiftValue = 0;
            string GiftCode = "";
            string GameCode = "";
            string Serial = "";
            DateTime GiftFrom = DateTime.Now;
            DateTime GiftTo = DateTime.Now;
            int Balance = 0;
            int ResponseStatus = -100;
            if (LoginConfig.AccountId > 0)
            {
                AbstractDAOFactory.Instance().CreateAccountVIPDAO().SP_VTCGameRollUp(LoginConfig.AccountId, LoginConfig.AccountName, ref GiftName, ref GiftValue, ref GiftCode, ref GameCode, ref Serial, ref GiftFrom, ref GiftTo, ref Balance, ref ResponseStatus);
            }

            return Json(new { ResponseStatus = ResponseStatus }, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult Mission()
        {
            return PartialView();
        }

        public ActionResult CallBackAcceptFace()
        {
            return View();
        }

        public ActionResult TabHelp()
        {
            return PartialView();
        }

        #region Wap
        public ActionResult Wap_Index()
        {
            ViewBag.flag = 1;
            return View();
        }

        public ActionResult WapUnVerifyPhone()
        {
            var mobileInfo = AccountApi.GetAccountMobile();
            if (mobileInfo != null && !(mobileInfo.status == 1 || mobileInfo.status == 4))
            {
                return Redirect("/thong-tin-tai-khoan");
            }
            var accDetail = AccountApi.GetAccountDetail();
            if (accDetail == null)
            {
                FormsAuthentication.SignOut();
                SessionsManager.RemoveAllCookie();
                Session.Abandon();
                Response.Redirect(Config.UrlRoot);
            }
            return View(accDetail);
        }

        public ActionResult WapVerifyPhone()
        {
            var mobileInfo = AccountApi.GetAccountMobile();
            if (mobileInfo != null && (mobileInfo.status == 1 || mobileInfo.status == 4))
            {
                return Redirect("/thong-tin-tai-khoan");
            }
            var accDetail = AccountApi.GetAccountDetail();
            if (accDetail == null)
            {
                FormsAuthentication.SignOut();
                SessionsManager.RemoveAllCookie();
                Session.Abandon();
                Response.Redirect(Config.UrlRoot);
            }
            return View(accDetail);
        }

        public ActionResult WapChangePhone()
        {
            var mobileInfo = AccountApi.GetAccountMobile();
            if (mobileInfo != null && !(mobileInfo.status == 1 || mobileInfo.status == 4))
            {
                return Redirect("/thong-tin-tai-khoan");
            }
            var accDetail = AccountApi.GetAccountDetail();
            if (accDetail == null)
            {
                FormsAuthentication.SignOut();
                SessionsManager.RemoveAllCookie();
                Session.Abandon();
                Response.Redirect(Config.UrlRoot);
            }
            return View(accDetail);
        }

        public ActionResult WapChangeMail()
        {
            var accDetail = AccountApi.GetAccountDetail();
            if (accDetail == null)
            {
                FormsAuthentication.SignOut();
                SessionsManager.RemoveAllCookie();
                Session.Abandon();
                Response.Redirect(Config.UrlRoot);
            }
            return View(accDetail);
        }

        public ActionResult WapChuyenKhoan()
        {
            var accDetail = AccountApi.GetAccountDetail();
            if (accDetail == null)
            {
                FormsAuthentication.SignOut();
                SessionsManager.RemoveAllCookie();
                Session.Abandon();
                Response.Redirect(Config.UrlRoot);
            }
            return View(accDetail);
        }
        #endregion
    }
}
