﻿namespace RankTangle.Controllers
{
    using System;
    using System.Web;
    using System.Web.Mvc;
    using MongoDB.Bson;
    using MongoDB.Driver.Builders;
    using RankTangle.ControllerHelpers;
    using RankTangle.Main;
    using RankTangle.Models.Domain;
    using RankTangle.Models.ViewModels;

    public class AccountController : BaseController
    {
        public static string GetAuthToken(Player player)
        {
            return Md5.CalculateMd5(player.Id + player.Email + "RankTangle4Ever");
        }

        // COOKIE DOUGH
        public void CreateRememberMeCookie(Player player)
        {
            HttpContext.Response.Cookies.Add(new HttpCookie("RankTangleAuth"));
            var httpCookie = ControllerContext.HttpContext.Response.Cookies["RankTangleAuth"];
            if (httpCookie != null)
            {
                httpCookie["Token"] = GetAuthToken(player);
            }

            if (httpCookie != null)
            {
                httpCookie.Expires = DateTime.Now.AddDays(30);
            }
        }

        public void RemoveRememberMeCookie()
        {
            ControllerContext.HttpContext.Response.Cookies.Add(new HttpCookie("RankTangleAuth"));
            var httpCookie = ControllerContext.HttpContext.Response.Cookies["RankTangleAuth"];
            if (httpCookie != null)
            {
                httpCookie.Expires = DateTime.Now.AddDays(-1);
            }
        }

        public bool Login(Player player)
        {
            // Set or remove cookie for future auto-login
            if (player != null)
            {
                if (player.RememberMe)
                {
                    // Save an autologin token as cookie and in the Db
                    var playerCollection = this.Dbh.GetCollection<Player>("Players");
                    var autoLoginCollection = this.Dbh.GetCollection<AutoLogin>("AutoLogin");
                    var autoLogin = autoLoginCollection.FindOne(Query.EQ("Email", player.Email));

                    if (autoLogin == null)
                    {
                        autoLogin = new AutoLogin
                        {
                            Email = player.Email,
                            Token = GetAuthToken(player),
                            Created = DateTime.Now
                        };
                        autoLoginCollection.Save(autoLogin);
                    }

                    CreateRememberMeCookie(player);
                    player.RememberMe = player.RememberMe;
                    playerCollection.Save(player);
                }
                else
                {
                    RemoveRememberMeCookie();
                }

                this.Session["Admin"] = this.Settings.AdminAccount == player.Email;
                this.Session["IsLoggedIn"] = true;
                this.Session["User"] = player;

                return true;
            }

            return false;
        }

        public ActionResult LogOn()
        {
            if (Session["IsLoggedIn"] == null || Session["IsLoggedIn"].ToString() == "false")
            {
                var authCookie = this.Request.Cookies.Get("RankTangleAuth");
                if (authCookie != null && authCookie["Token"] != null)
                {
                    var autoLoginCollection = this.Dbh.GetCollection<AutoLogin>("AutoLogin");
                    var autoLoginToken = autoLoginCollection.FindOne(Query.EQ("Token", authCookie["Token"]));

                    if (autoLoginToken != null) 
                    {
                        var playerCollection = this.Dbh.GetCollection<Player>("Players");
                        var player = playerCollection.FindOne(Query.EQ("Email", autoLoginToken.Email.ToLower()));
                    
                        if (Login(player))
                        {
                            // Go back to where we were before logging in
                            var referrer = this.Request.UrlReferrer;
                            if (referrer != null)
                            {
                                return this.Redirect(referrer.ToString());
                            }
                        } 
                    }
                }
            }

            var urlReferrer = this.Request.UrlReferrer;

            if (urlReferrer != null)
            {
                var viewModel = new LogOnViewModel { RefUrl = urlReferrer.ToString(), Settings = this.Settings };

                return this.View(viewModel);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult LogOn(LogOnViewModel model)
        {
            model.Settings = this.Settings;
            var email = model.Email.ToLower();
            if (this.Settings.EnableDomainValidation)
            {
                email += "@" + this.Settings.Domain;
            }

            var playerCollection = this.Dbh.GetCollection<Player>("Players");
            var player = playerCollection.FindOne(Query.EQ("Email", email));
            
            if (player != null)
            {
                if (player.Password == Md5.CalculateMd5(model.Password))
                {
                    if (Login(player))
                    {
                        return Redirect(model.RefUrl);
                    }
                }
            }

            model.LogOnError = true;
            return View(model);
        }

        // GET: /Account/LogOff
        public ActionResult LogOff()
        {
            Session.Clear();
            RemoveRememberMeCookie();
            
            // Go back to where we were before logging in
            var urlReferrer = this.Request.UrlReferrer;
            if (urlReferrer != null)
            {
                return this.Redirect(urlReferrer.ToString());
            } 
                
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Register
        public ActionResult Register()
        {
            var viewModel = new PlayerBaseDataViewModel
                                {
                                    Player = new Player(),
                                    Settings = this.Settings
                                };
            return View(viewModel);
        }

        // POST: /Account/Register
        [HttpPost]
        public ActionResult Register(PlayerBaseDataViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var email = viewModel.Player.Email.ToLower();
                if (this.Settings.EnableDomainValidation)
                {
                    email += "@" + this.Settings.Domain;
                }

                var name = viewModel.Player.Name;
                var password = Md5.CalculateMd5(viewModel.Player.Password);
                var nickname = viewModel.Player.NickName;
                var gender = viewModel.Player.Gender;
            
                var playerCollection = this.Dbh.GetCollection<Player>("Players");

                var newPlayer = new Player
                                    {
                                        Id = BsonObjectId.GenerateNewId().ToString(),
                                        Email = email,
                                        Name = name,
                                        Gender = gender,
                                        Password = password,
                                        NickName = nickname,
                                        Won = 0,
                                        Lost = 0,
                                        Played = 0
                                    };

                playerCollection.Save(newPlayer);

                Login(newPlayer);
                var restResponse = Email.SendSimpleEmail();

                return this.Redirect(Url.Action("Index", "Players") + "#" + newPlayer.Id);
            }

            return this.View("Register");
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            var currentUser = (Player)Session["User"];
            var player = DbHelper.GetPlayer(id);
            
            if (currentUser != null && (currentUser.Id == player.Id || currentUser.Email == this.Settings.AdminAccount))
            {
                var refUrl = HttpContext.Request.UrlReferrer != null
                                 ? HttpContext.Request.UrlReferrer.AbsoluteUri
                                 : "/Players";
                return this.View(new PlayerBaseDataViewModel
                                     {
                                         Player = player, 
                                         Settings = this.Settings,
                                         ReferralUrl = refUrl
                                     });
            }

            return this.RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Edit(PlayerBaseDataViewModel viewModel)
        {
            viewModel.SaveSuccess = false;

            if (ModelState.IsValid)
            {
                var currentUser = (Player)Session["User"];

                if (this.Settings.EnableDomainValidation)
                {
                    viewModel.Player.Email += "@" + this.Settings.Domain;
                }

                if (currentUser != null
                    && (currentUser.Id == viewModel.Player.Id || currentUser.Email == this.Settings.AdminAccount))
                {
                    var player = DbHelper.GetPlayer(viewModel.Player.Id);
                    var gender = viewModel.Player.Gender;

                    player.Email = string.IsNullOrEmpty(viewModel.Player.Email)
                                        ? player.Email
                                        : viewModel.Player.Email;
                    player.Name = string.IsNullOrEmpty(viewModel.Player.Name)
                                        ? player.Name
                                        : viewModel.Player.Name;
                    player.Gender = gender;
                    player.Password = string.IsNullOrEmpty(viewModel.Player.Password)
                                        ? player.Password
                                        : Md5.CalculateMd5(viewModel.Player.Password);
                    player.NickName = string.IsNullOrEmpty(viewModel.Player.NickName)
                                        ? player.NickName
                                        : viewModel.Player.NickName;

                    DbHelper.SavePlayer(player);
                    viewModel.Player = player;
                    viewModel.SaveSuccess = true;
                }

                viewModel.Settings = this.Settings;
                return this.View("Edit", viewModel);
            }

            return this.RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public JsonResult PlayerEmailIsValid(string email)
        {
            var query = Query.EQ("Email", email.ToLower());
            var playerCollection = this.Dbh.GetCollection<Player>("Players");
            var player = playerCollection.FindOne(query);

            if (player != null)
            {
                return Json(new ExistsResponse { Exists = true, Name = player.Name, Email = player.Email });
            }
    
            return Json(new ExistsResponse { Exists = false, Name = null, Email = null });
        }

        // POST: /Account/PlayerNameExists
        [HttpPost]
        public JsonResult PlayerNameExists(string name)
        {
            var playerCollection = this.Dbh.GetCollection<Player>("Players");
            var query = Query.EQ("Name", name);
            var player = playerCollection.FindOne(query);

            if (player != null)
            {
                return Json(new ExistsResponse { Exists = true, Name = player.Name, Email = player.Email });
            }

            return Json(new ExistsResponse { Exists = false, Name = null, Email = null });
        }

        // GET Account/GetGravatarUrl/{emailPrefix}
        [HttpGet]
        public JsonResult GetGravatarUrl(string email)
        {
            if (!string.IsNullOrEmpty(email))
            {
                var gravatarUrl = Md5.GetGravatarEmailHash(email);
                return Json(new { url = gravatarUrl }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { url = string.Empty }, JsonRequestBehavior.AllowGet);
        }
    }
}
