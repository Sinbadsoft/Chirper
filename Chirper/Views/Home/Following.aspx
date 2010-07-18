<%@ Page Title="" Language="C#" Inherits="System.Web.Mvc.ViewPage<System.Collections.Generic.IEnumerable<JavaGeneration.Chirper.Models.Following>>" MasterPageFile="~/Views/Shared/Site.Master" %>
<asp:Content runat="server" ID="Title" ContentPlaceHolderID="TitleContent">Following</asp:Content>
<asp:Content runat="server" ID="Main" ContentPlaceHolderID="MainContent">
     <table>
        <tr>
            <th>User</th>
            <th>Name</th>
            <th>Bio</th>
            <th>Since</th>
        </tr>
        <% foreach (var item in Model)
           {%>
        <tr>
            <td><%=Html.ActionLink(item.User.Name, "UserLine", new { id = item.User.Name })%></td>
            <td><%=Html.Encode(item.User.DisplayName)%></td>
            <td><%=Html.Encode(item.User.Bio)%></td>
            <td><%=Html.Encode(string.Format("{0:g}", item.Since))%></td>
        </tr>
        <% } %>
    </table>
</asp:Content>
