<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<JavaGeneration.Chirper.Models.User>" %>
<p>
    <label for="email">Email:</label>
    <%= Html.TextBox("email", Model != null ? Model.Email : string.Empty) %>
    <%= Html.ValidationMessage("email") %>
</p>
<p>
    <label for="displayName">Display Name:</label>
    <%= Html.TextBox("displayName", Model != null ? Model.DisplayName : string.Empty)%>
    <%= Html.ValidationMessage("displayName")%>
</p>
<p>
    <label for="web">Website:</label>
    <%= Html.TextBox("web", Model != null ? Model.Web : string.Empty)%>
    <%= Html.ValidationMessage("web")%>
</p>

<p>
    <label for="location">Location:</label>
    <%= Html.TextBox("location", Model != null ? Model.Location : string.Empty)%>
    <%= Html.ValidationMessage("location")%>
</p>

<p>
    <label for="bio">Bio:</label>
    <%= Html.TextBox("bio", Model != null ? Model.Bio : string.Empty)%>
    <%= Html.ValidationMessage("bio")%>
</p>
