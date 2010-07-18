<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<JavaGeneration.Chirper.Models.Tweet>>" %>
<%@ Import Namespace="JavaGeneration.Chirper.Models" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	<%= Html.Encode(ViewData["Title"]) %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <%
        if (User.Identity.IsAuthenticated)
        {
            using (Html.BeginForm("Tweet", "Home"))
            {%>
    <p>
        <label for="text">What's happening?</label><br />
        <%=Html.TextArea("text", string.Empty, 3, 50, new { @class = "tweet-post" })%>
    </p>
    <p>
        <input id="tweet" type="submit" value="Tweet" /></p>
    <%
        }
        }%>
    <h2><%= Html.Encode(ViewData["Title"]) %></h2>
    <%
        var loggedUser = (User)ViewData["LoggedUser"];
        var user = (User)ViewData["User"];
        if (loggedUser != null && user != null && !string.Equals(user.Name, loggedUser.Name))
        {%>
        <%=Html.ActionLink("Follow", "Follow", new { id = user.Name })%>
        <%}%>
    <table>
        <tr>
            <th>User</th>
            <th>Text</th>
            <th>Location</th>
            <th>Time</th>
            <th>In Reply To</th>
        </tr>
        <% foreach (var item in Model)
           {%>
        <tr>
            <td><%=Html.ActionLink(item.User, "UserLine", new { id = item.User })%></td>
            <td><%=Html.Encode(item.Text)%></td>
            <td><%=Html.Encode(item.Location)%></td>
            <td><%=Html.Encode(string.Format("{0:g}", item.Time))%></td>
            <% if (!string.IsNullOrEmpty(item.InReplyToUser) && string.IsNullOrEmpty(item.InReplyToTweet)) 
               {%>
            <td><%=Html.ActionLink(item.InReplyToUser, "Tweet", new { tweet = item.Id })%></td>
            <% }
               else
               {%>
            <td></td>
            <% }%>
        </tr>
        <% } %>
    </table>
</asp:Content>