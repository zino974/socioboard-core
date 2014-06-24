﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using GlobusTwitterLib.Authentication;
using System.Configuration;
using GlobusInstagramLib.Instagram.Core;
using GlobusLinkedinLib.Authentication;
using GlobusInstagramLib.Instagram.Core;
using GlobusTwitterLib.Twitter.Core.UserMethods;
using Facebook;
using SocioBoard.Domain;
using SocioBoard.Model;

namespace SocioBoard.Setting
{

    public partial class UsersAndGroups : System.Web.UI.Page
    {
        GroupRepository grouprepo = new GroupRepository();
        protected void page_load(object sender, EventArgs e)
        {
            User user = (User)Session["LoggedUser"];

            if (user == null)
                Response.Redirect("/Login.aspx");

            if (!IsPostBack)
            {


                List<Groups> lstgroup = grouprepo.getAllGroups(user.Id);
                ddlGroup.DataSource = lstgroup;
                ddlGroup.DataTextField = "GroupName";
                ddlGroup.DataValueField = "Id";
                ddlGroup.DataBind();
                ddlGroup.Items.Insert(0, "Select");
                this.GetAllGroups(lstgroup);
                if (!AllGroups.InnerHtml.ToString().Contains("<span id=\"totalgroups\" style=\"display:none;\">0</span>"))
                {
                    this.ProfilesAvailabeforuser(user.Id);
                }
            }

        }


        public void TwitterOAuthRedirect(object sender, EventArgs e)
        {
            User user = (User)Session["LoggedUser"];
         
            oAuthTwitter OAuth = new oAuthTwitter();

            if (ddlGroup.SelectedIndex > 0)
            {
                HiddenFieldGroupNameInDDl.Value = ddlGroup.SelectedItem.Text;

                if (!string.IsNullOrEmpty(HiddenFieldGroupNameInDDl.Value))
                {
                    GroupRepository grouprepo = new GroupRepository();
                    Groups group = grouprepo.getGroupDetails(user.Id, HiddenFieldGroupNameInDDl.Value.ToString());
                    Session["GroupName"] = group;
                    if (Request["oauth_token"] == null)
                    {
                        Session["UserAndGroupsForTwitter"] = "twitter";
                        OAuth.AccessToken = string.Empty;
                        OAuth.AccessTokenSecret = string.Empty;
                        OAuth.CallBackUrl = ConfigurationManager.AppSettings["callbackurl"].ToString();
                        this.TwitterOAuth.HRef = OAuth.AuthorizationLinkGet();
                        Response.Redirect(OAuth.AuthorizationLinkGet());
                    }
                }
            }
            else
            {
                try
                {
                    string txtgroup = Page.Request.Form["txtGroupName"].ToString();

                   

                    if (!string.IsNullOrEmpty(txtgroup))
                    {
                        GroupRepository grouprepo = new GroupRepository();
                        Groups group = new Groups();
                        group.Id = Guid.NewGuid();
                        group.GroupName = txtgroup;
                        group.UserId = user.Id;
                        group.EntryDate = DateTime.Now;
                        if (!grouprepo.checkGroupExists(user.Id, txtgroup))
                        {
                            grouprepo.AddGroup(group);
                            Groups grou = grouprepo.getGroupDetails(user.Id, txtgroup);
                            Session["GroupName"] = grou;
                        }
                        else
                        {
                            Groups grou = grouprepo.getGroupDetails(user.Id, txtgroup);
                            Session["GroupName"] = grou;
                        }
                        if (Request["oauth_token"] == null)
                        {
                            Session["UserAndGroupsForTwitter"] = "twitter";
                            OAuth.AccessToken = string.Empty;
                            OAuth.AccessTokenSecret = string.Empty;
                            OAuth.CallBackUrl = ConfigurationManager.AppSettings["callbackurl"].ToString();
                            this.TwitterOAuth.HRef = OAuth.AuthorizationLinkGet();
                            Response.Redirect(OAuth.AuthorizationLinkGet());
                        }
                    }
                    else
                    {
                        Response.Write("<script>alert(\"Please Add new Group or Select Existing Group\");</script>");
                    }
                  
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                }
            }

            
        }

        public void FacebookRedirect(object sender, EventArgs e)
        {
            User user = (User)Session["LoggedUser"];

            if (ddlGroup.SelectedIndex > 0)
            {
                HiddenFieldGroupNameInDDl.Value = ddlGroup.SelectedItem.Text;
                if (!string.IsNullOrEmpty(HiddenFieldGroupNameInDDl.Value))
                {
                    GroupRepository grouprepo = new GroupRepository();
                    Groups group = grouprepo.getGroupDetails(user.Id, HiddenFieldGroupNameInDDl.Value.ToString());
                    Session["GroupName"] = group;
                    Session["UserAndGroupsForFacebook"] = "facebook";
                    facebook_connect.HRef = "http://www.facebook.com/dialog/oauth/?scope=publish_stream,read_stream,read_insights,manage_pages,user_checkins,user_photos,read_mailbox,manage_notifications,read_page_mailboxes,email,user_videos,offline_access&client_id=" + ConfigurationManager.AppSettings["ClientId"] + "&redirect_uri=" + ConfigurationManager.AppSettings["RedirectUrl"] + "&response_type=code";
                    FbOuthForProfile.HRef = facebook_connect.HRef;
                    Response.Redirect(facebook_connect.HRef);
                }
            }
            else
            {
                try
                {
                    string txtgroup = Page.Request.Form["txtGroupName"].ToString();

                    if (!string.IsNullOrEmpty(txtgroup))
                    {
                        GroupRepository grouprepo = new GroupRepository();
                        Groups group = new Groups();
                        group.Id = Guid.NewGuid();
                        group.GroupName = txtgroup;
                        group.UserId = user.Id;
                        group.EntryDate = DateTime.Now;
                        if (!grouprepo.checkGroupExists(user.Id, txtgroup))
                        {
                            grouprepo.AddGroup(group);
                            Groups grou = grouprepo.getGroupDetails(user.Id, txtgroup);
                            Session["GroupName"] = grou;
                        }
                        else
                        {
                            Groups grou = grouprepo.getGroupDetails(user.Id, txtgroup);
                            Session["GroupName"] = grou;
                        }
                        Session["UserAndGroupsForFacebook"] = "facebook";
                        facebook_connect.HRef = "http://www.facebook.com/dialog/oauth/?scope=publish_stream,read_stream,read_insights,manage_pages,user_checkins,user_photos,read_mailbox,manage_notifications,read_page_mailboxes,email,user_videos,offline_access&client_id=" + ConfigurationManager.AppSettings["ClientId"] + "&redirect_uri=" + ConfigurationManager.AppSettings["RedirectUrl"] + "&response_type=code";
                        FbOuthForProfile.HRef = facebook_connect.HRef;
                        Response.Redirect(facebook_connect.HRef);

                    }
                    else
                    { 
                     Response.Write("<script>alert(\"Please Add new Group or Select Existing Group\");</script>");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace); 
                }
            }
           
        }

        public void GetAllGroups(List<Groups> lstgroup)
        {

            string bindgroups = string.Empty;
            int i = 0;
            bindgroups += "  <ul class=\"square-bubble\" id=\"groups-list\">";
            foreach (Groups item in lstgroup)
            {

                if (i == 0)
                {
                    bindgroups += "<li runat=\"server\" onclick=\"changeClassandProfilesOfGroup(this.id);\" onserverclick=\"BindAllProfilesAccordingToGroup(this.id)\" id=\"group_" + i + "\" class=\"fifth standard selected puff\"><span id=\"itemid_" + i + "\" style=\"display:none;\">" + item.Id + "</span>" +
                                    "<div class=\"folder_green group_sprite grp hide-text\">" + item.GroupName + "</div>" +
               "<a href=\"javascript:void(0);\" class=\"grp_name\">" +
                "<span class=\"text\">" + item.GroupName + "</span>" +
                     "<input id=\"groupname_" + i + "\" type=\"text\" value=\"" + item.GroupName + "\" class=\"grpNameInput\">" +
               "</a><a onclick=\"DeleteGroup('"+item.Id+"',"+i+")\" title=\"Remove this group\" class=\"delete_group delete_btn trash\" href=\"javascript:void(0);\">Delete Group</a>" +
          "</li>";

                }
                else
                {
                    bindgroups += "<li onclick=\"changeClassandProfilesOfGroup(this.id);\" id=\"group_" + i + "\" class=\"fifth standard\"><span id=\"itemid_" + i + "\" style=\"display:none;\">" + item.Id + "</span>" +
                                   "<div class=\"folder_green group_sprite grp hide-text\">" + item.GroupName + "</div>" +
              "<a href=\"javascript:void(0);\" class=\"grp_name\">" +
               "<span class=\"text\">" + item.GroupName + "</span>" +
                    "<input id=\"groupname_" + i + "\" type=\"text\" value=\"" + item.GroupName + "\" class=\"grpNameInput\">" +
              "</a><a onclick=\"DeleteGroup('" + item.Id + "',"+i+")\" title=\"Remove this group\" class=\"delete_group delete_btn trash\" href=\"javascript:void(0);\">Delete Group</a>" +
         "</li>";

                }

                i++;
            }
            bindgroups += "</ul><span id=\"totalgroups\" style=\"display:none;\">" + i + "</span>";
            AllGroups.InnerHtml = bindgroups;
        }

        public void ProfilesAvailabeforuser(Guid UserId)
        {
            string bindprofiles = string.Empty;
         
            SocialProfilesRepository socioprofilerepo = new SocialProfilesRepository();
            List<SocialProfile>  lstsocialprofile =     socioprofilerepo.getAllSocialProfilesOfUser(UserId);
            foreach (SocialProfile item in lstsocialprofile)
            {

                if (item.ProfileType == "facebook")
                {
                    if (!SelectedGroupProfiles.InnerHtml.Contains("facebook_" + item.ProfileId))
                    {
                        FacebookAccountRepository fbaccreop = new FacebookAccountRepository();
                        FacebookAccount facebookaccount = fbaccreop.getFacebookAccountDetailsById(item.ProfileId, UserId);

                        bindprofiles +=
                                       "<div id=\"usergroups_"+item.ProfileId+"\" class=\"ws_conct active\"> <span class=\"img\"><img width=\"48\" height=\"48\" src=\"http://graph.facebook.com/" + item.ProfileId + "/picture?type=small\" alt=\"\"><i><img width=\"16\" height=\"16\" src=\"../Contents/Images/fb_icon.png\" alt=\"\"></i></span><div class=\"fourfifth\">" +
                                       "<div class=\"location-container\">" + facebookaccount.FbUserName + "</div><span class=\"add remove\">✖</span></div></div>";
                    }
                }
                else if (item.ProfileType == "twitter")
                {
                    if (!SelectedGroupProfiles.InnerHtml.Contains("twitter_" + item.ProfileId))
                    {
                        string profileimgurl = string.Empty;
                        TwitterAccountRepository twtaccountrepo = new TwitterAccountRepository();
                        TwitterAccount twtacco = twtaccountrepo.getUserInformation(UserId, item.ProfileId);
                        if (twtacco.ProfileImageUrl == string.Empty)
                        {
                            profileimgurl = "../../Contents/Images/blank_img.png";
                        }
                        else
                        {
                            profileimgurl = twtacco.ProfileImageUrl;
                        }
                        bindprofiles +=
                                     "<div id=\"usergroups_"+item.ProfileId+"\" class=\"ws_conct active\"> <span class=\"img\"><img width=\"48\" height=\"48\" src=\"" + profileimgurl + "\" alt=\"\"><i><img width=\"16\" height=\"16\" src=\"../Contents/Images/twticon.png\" alt=\"\"></i></span><div class=\"fourfifth\">" +
                                     "<div class=\"location-container\">" + twtacco.TwitterName + "</div><span class=\"add remove\">✖</span></div></div>";
                    }
                }
                else if (item.ProfileType == "linkedin")
                {
                    if (!SelectedGroupProfiles.InnerHtml.Contains("linkedin_" + item.ProfileId))
                    {
                        LinkedInAccountRepository linkedaccrepo = new LinkedInAccountRepository();
                        LinkedInAccount linkedaccount = linkedaccrepo.getUserInformation(UserId, item.ProfileId);
                        string profileimgurl = string.Empty;
                        if (linkedaccount.ProfileUrl == string.Empty)
                        {
                            profileimgurl = "../../Contents/Images/blank_img.png";
                        }
                        else
                        {
                            profileimgurl = linkedaccount.ProfileUrl;
                        }
                        bindprofiles += "<div id=\"usergroups_"+item.ProfileId+"\" class=\"ws_conct active\"><span class=\"img\"><img width=\"48\" height=\"48\" alt=\"\" src=\"" + profileimgurl + "\" ><i>" +
                                         "<img width=\"16\" height=\"16\" alt=\"\" src=\"../Contents/Images/link_icon.png\"></i></span>" +
                                         "<div class=\"fourfifth\"><div class=\"location-container\">" + linkedaccount.LinkedinUserName + "</div>" +
                                         "<span class=\"add remove\">✖</span></div></div>";
                    }
                }
                else if (item.ProfileType == "instagram")
                {
                    if (!SelectedGroupProfiles.InnerHtml.Contains("instagram_" + item.ProfileId))
                    {
                        string profileimgurl = string.Empty;
                        InstagramAccountRepository instagramrepo = new InstagramAccountRepository();
                        InstagramAccount instaaccount = instagramrepo.getInstagramAccountDetailsById(item.ProfileId, UserId);
                        if (instaaccount.ProfileUrl == string.Empty)
                        {
                            profileimgurl = "../../Contents/Images/blank_img.png";
                        }
                        else
                        {
                            profileimgurl = instaaccount.ProfileUrl;
                        }

                        bindprofiles += "<div id=\"usergroups_"+item.ProfileId+"\" class=\"ws_conct active\"><span class=\"img\"><img width=\"48\" height=\"48\" src=\"" + profileimgurl + "\" alt=\"\"><i>" +
                                          "<img width=\"16\" height=\"16\" alt=\"\" src=\"../Contents/Images/instagram_24X24.png\"></i></span><div class=\"fourfifth\"><div class=\"location-container\">" + instaaccount.InsUserName + "</div>" +
                            "<span class=\"add remove\">✖</span></div></div>";
                    }
                }

            }
            AllGroupProfiles.InnerHtml = bindprofiles;
        }
    }
}