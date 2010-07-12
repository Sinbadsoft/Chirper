<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<JavaGeneration.Chirper.Models.Tweet>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	TimeLine
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <%
        if (User.Identity.IsAuthenticated)
        {
            using (Html.BeginForm("Tweet", "Home"))
            {%>
    <p>
        <label for="text" style="">
            What’s happening?</label><br />
        <%=Html.TextArea("text", string.Empty, 3, 50, new { @class = "tweet-post" })%>
    </p>
    <p>
        <input id="tweet" type="submit" value="Tweet" /></p>
    <%
        }
        }%>
    <h2>TimeLine</h2>

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
            <td><%=Html.Encode(item.User)%></td>
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
