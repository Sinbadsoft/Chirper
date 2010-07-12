<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%
    if (Request.IsAuthenticated) {
%>
        Welcome <b><%= Html.Encode(Page.User.Identity.Name) %></b>!
        <small>[ <%= Html.ActionLink("Log Off", "LogOff", "Account")%> ]</small>
<%
    }
    else 
    {       
%>

        <%= Html.ActionLink("Sign Up!", "Register", "Account") %>
        <small>Already registered? [ <%= Html.ActionLink("Sign In", "LogOn", "Account") %> ]</small>
<%
    }
%>
