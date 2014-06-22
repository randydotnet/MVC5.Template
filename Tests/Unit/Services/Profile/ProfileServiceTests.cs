﻿using NUnit.Framework;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Template.Components.Alerts;
using Template.Components.Security;
using Template.Data.Core;
using Template.Objects;
using Template.Resources.Views.ProfileView;
using Template.Services;
using Template.Tests.Data;
using Template.Tests.Helpers;

namespace Template.Tests.Unit.Services
{
    [TestFixture]
    public class ProfileServiceTests
    {
        private ProfileService service;
        private AContext context;
        private Account account;

        [SetUp]
        public void SetUp()
        {
            context = new TestingContext();
            HttpMock httpMock = new HttpMock();
            HttpContext.Current = httpMock.HttpContext;
            service = new ProfileService(new UnitOfWork(context));
            httpMock.IdentityMock.Setup(mock => mock.Name).Returns(() => account.Id);

            service.ModelState = new ModelStateDictionary();
            service.AlertMessages = new MessagesContainer();

            TearDownData();
            SetUpData();
        }

        [TearDown]
        public void TearDown()
        {
            HttpContext.Current = null;

            service.Dispose();
            context.Dispose();
        }

        #region Method: AccountExists(String accountId)

        [Test]
        public void AccountExists_ReturnsTrueIfAccountExistsInDatabase()
        {
            Assert.IsTrue(service.AccountExists(account.Id));
        }

        [Test]
        public void AccountExists_ReturnsFalseIfAccountDoesNotExistInDatabase()
        {
            Assert.IsFalse(service.AccountExists("Test"));
        }

        #endregion

        #region Method: CanEdit(ProfileView profile)

        [Test]
        public void CanEdit_CanNotEditWithInvalidModelState()
        {
            service.ModelState.AddModelError("Key", "ErrorMessages");

            Assert.IsFalse(service.CanEdit(ObjectFactory.CreateProfileView()));
        }

        [Test]
        public void CanEdit_CanNotEditWithIncorrectPassword()
        {
            ProfileView profile = ObjectFactory.CreateProfileView();
            profile.CurrentPassword += "1";

            Assert.IsFalse(service.CanEdit(profile));
        }

        [Test]
        public void CanEdit_AddsErrorMessageThenCanNotEditWithIncorrectPassword()
        {
            ProfileView profile = ObjectFactory.CreateProfileView();
            profile.CurrentPassword += "1";
            service.CanEdit(profile);

            String expected = Validations.IncorrectPassword;
            String actual = service.ModelState["CurrentPassword"].Errors[0].ErrorMessage;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanEdit_CanNotEditToAlreadyTakenUsername()
        {
            Account takenAccount = ObjectFactory.CreateAccount();
            takenAccount.Username += "1";
            takenAccount.Id += "1";

            context.Set<Account>().Add(takenAccount);
            context.SaveChanges();

            ProfileView profile = ObjectFactory.CreateProfileView();
            profile.Username = takenAccount.Username;

            Assert.IsFalse(service.CanEdit(profile));
        }

        [Test]
        public void CanEdit_AddsErrorMessageThenCanNotEditToAlreadyTakenUsername()
        {
            Account takenAccount = ObjectFactory.CreateAccount();
            takenAccount.Username += "1";
            takenAccount.Id += "1";

            context.Set<Account>().Add(takenAccount);
            context.SaveChanges();

            ProfileView profile = ObjectFactory.CreateProfileView();
            profile.Username = takenAccount.Username;
            service.CanEdit(profile);

            String expected = Validations.UsernameIsAlreadyTaken;
            String actual = service.ModelState["Username"].Errors[0].ErrorMessage;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanEdit_CanEditUsingItsOwnUsername()
        {
            ProfileView profile = ObjectFactory.CreateProfileView();
            profile.Username = profile.Username.ToUpper();

            Assert.IsTrue(service.CanEdit(profile));
        }

        [Test]
        public void CanEdit_CanNotEditIfNewPasswordIsTooShort()
        {
            ProfileView profile = ObjectFactory.CreateProfileView();
            profile.NewPassword = "AaaAaa1";

            Assert.IsFalse(service.CanEdit(profile));
            Assert.AreEqual(service.ModelState["NewPassword"].Errors[0].ErrorMessage, Validations.IllegalPassword);
        }

        [Test]
        public void CanEdit_AddsErrorMessageThenCanNotEditIfNewPasswordIsTooShort()
        {
            ProfileView profile = ObjectFactory.CreateProfileView();
            profile.NewPassword = "AaaAaa1";
            service.CanEdit(profile);

            String expected = Validations.IllegalPassword;
            String actual = service.ModelState["NewPassword"].Errors[0].ErrorMessage;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanEdit_CanNotEditIfNewPasswordDoesNotContainUpperLetter()
        {
            ProfileView profile = ObjectFactory.CreateProfileView();
            profile.NewPassword = "aaaaaaaaaaaa1";

            Assert.IsFalse(service.CanEdit(profile));
        }

        [Test]
        public void CanEdit_AddsErrorMessageThenCanNotEditIfNewPasswordDoesNotContainUpperLetter()
        {
            ProfileView profile = ObjectFactory.CreateProfileView();
            profile.NewPassword = "aaaaaaaaaaaa1";
            service.CanEdit(profile);

            String expected = Validations.IllegalPassword;
            String actual = service.ModelState["NewPassword"].Errors[0].ErrorMessage;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanEdit_CanNotEditIfNewPasswordDoesNotContainLowerLetter()
        {
            ProfileView profile = ObjectFactory.CreateProfileView();
            profile.NewPassword = "AAAAAAAAAAA1";

            Assert.IsFalse(service.CanEdit(profile));
        }

        [Test]
        public void CanEdit_AddsErrorMessageThenCanNotEditIfNewPasswordDoesNotContainLowerLetter()
        {
            ProfileView profile = ObjectFactory.CreateProfileView();
            profile.NewPassword = "AAAAAAAAAAA1";
            service.CanEdit(profile);

            String expected = Validations.IllegalPassword;
            String actual = service.ModelState["NewPassword"].Errors[0].ErrorMessage;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanEdit_CanNotEditIfNewPasswordDoesNotContainADigit()
        {
            ProfileView profile = ObjectFactory.CreateProfileView();
            profile.NewPassword = "AaAaAaAaAaAa";

            Assert.IsFalse(service.CanEdit(profile));
        }

        [Test]
        public void CanEdit_AddsErrorMessageThenCanNotEditIfNewPasswordDoesNotContainADigit()
        {
            ProfileView profile = ObjectFactory.CreateProfileView();
            profile.NewPassword = "AaAaAaAaAaAa";
            service.CanEdit(profile);

            String expected = Validations.IllegalPassword;
            String actual = service.ModelState["NewPassword"].Errors[0].ErrorMessage;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanEdit_CanEditWithoutSpecifyingNewPassword()
        {
            ProfileView profile = ObjectFactory.CreateProfileView();
            profile.NewPassword = null;

            Assert.IsTrue(service.CanEdit(profile));
        }

        [Test]
        public void CanEdit_CanNotEditWithNullEmail()
        {
            ProfileView profile = ObjectFactory.CreateProfileView();
            profile.Email = null;

            Assert.IsFalse(service.CanEdit(profile));
        }

        [Test]
        public void CanEdit_AddsErorrMessageThenCanNotEditWithNullEmail()
        {
            ProfileView profile = ObjectFactory.CreateProfileView();
            profile.Email = null;

            service.CanEdit(profile);

            String expected = String.Format(Template.Resources.Shared.Validations.FieldIsRequired, Titles.Email);
            String actual = service.ModelState["Email"].Errors[0].ErrorMessage;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanEdit_CanNotEditToAlreadyUsedEmail()
        {
            Account takenEmailAccount = ObjectFactory.CreateAccount();
            takenEmailAccount.Username += "1";
            takenEmailAccount.Id += "1";

            context.Set<Account>().Add(takenEmailAccount);
            context.SaveChanges();

            ProfileView profile = ObjectFactory.CreateProfileView();

            Assert.IsFalse(service.CanEdit(profile));
        }

        [Test]
        public void CanEdit_AddsErorrMessageThenCanNotEditToAlreadyUsedEmail()
        {
            Account takenEmailAccount = ObjectFactory.CreateAccount();
            takenEmailAccount.Username += "1";
            takenEmailAccount.Id += "1";

            context.Set<Account>().Add(takenEmailAccount);
            context.SaveChanges();

            ProfileView profile = ObjectFactory.CreateProfileView();
            service.CanEdit(profile);

            String expected = Validations.EmailIsAlreadyUsed;
            String actual = service.ModelState["Email"].Errors[0].ErrorMessage;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanEdit_CanEditUsingItsOwnEmail()
        {
            ProfileView profile = ObjectFactory.CreateProfileView();
            profile.Email = profile.Email.ToUpper();

            Assert.IsTrue(service.CanEdit(profile));
        }

        [Test]
        public void CanEdit_CanEditValidProfile()
        {
            Assert.IsTrue(service.CanEdit(ObjectFactory.CreateProfileView()));
        }

        #endregion

        #region Method: CanDelete(ProfileView profile)

        [Test]
        public void CanDelete_CanNotDeleteWithInvalidModelState()
        {
            service.ModelState.AddModelError("Test", "Test");

            Assert.IsFalse(service.CanDelete(ObjectFactory.CreateProfileView()));
        }

        [Test]
        public void CanDelete_CanNotDeleteWithIncorrectUsername()
        {
            ProfileView profile = ObjectFactory.CreateProfileView();
            profile.Username = String.Empty;

            Assert.IsFalse(service.CanDelete(profile));
        }

        [Test]
        public void CanDelete_AddsErrorMessageThenCanNotDeleteWithIncorrectUsername()
        {
            ProfileView profile = ObjectFactory.CreateProfileView();
            profile.Username = String.Empty;
            service.CanDelete(profile);

            String expected = Validations.IncorrectUsername;
            String actual = service.ModelState["Username"].Errors[0].ErrorMessage;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanDelete_CanNotDeleteWithIncorrectPassword()
        {
            ProfileView profile = ObjectFactory.CreateProfileView();
            profile.CurrentPassword += "1";

            Assert.IsFalse(service.CanDelete(profile));
        }

        [Test]
        public void CanDelete_AddsErrorMessageThenCanNotDeleteWithIncorrectPassword()
        {
            ProfileView profile = ObjectFactory.CreateProfileView();
            profile.CurrentPassword += "1";
            service.CanDelete(profile);

            String expected = Validations.IncorrectPassword;
            String actual = service.ModelState["CurrentPassword"].Errors[0].ErrorMessage;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanDelete_CanDeleteValidProfileView()
        {
            Assert.IsTrue(service.CanDelete(ObjectFactory.CreateProfileView()));
        }

        #endregion

        #region Method: Edit(ProfileView profile)

        [Test]
        public void Edit_EditsAccount()
        {
            ProfileView profileView = ObjectFactory.CreateProfileView();
            Account expected = context.Set<Account>().SingleOrDefault(acc => acc.Id == account.Id);
            profileView.Username += "1";
            service.Edit(profileView);

            Account actual = context.Set<Account>().SingleOrDefault(acc => acc.Id == account.Id);

            Assert.IsTrue(BCrypter.Verify(profileView.NewPassword, actual.Passhash));
            Assert.AreEqual(expected.EntityDate, actual.EntityDate);
            Assert.AreEqual(expected.Username, actual.Username);
        }

        [Test]
        public void Edit_LeavesCurrentPasswordAfterEditing()
        {
            ProfileView profileView = ObjectFactory.CreateProfileView();
            profileView.NewPassword = null;
            service.Edit(profileView);

            Account actual = context.Set<Account>().SingleOrDefault(acc => acc.Id == account.Id);

            Assert.IsTrue(BCrypter.Verify(profileView.CurrentPassword, actual.Passhash));
        }

        #endregion

        #region Method: Delete(String id)

        [Test]
        public void Delete_DeletesAccount()
        {
            if (context.Set<Account>().SingleOrDefault(acc => acc.Id == account.Id) == null)
                Assert.Inconclusive();

            service.Delete(account.Id);

            Assert.IsNull(context.Set<Account>().SingleOrDefault(acc => acc.Id == account.Id));
        }

        #endregion

        #region Method: AddDeleteDisclaimerMessage()

        [Test]
        public void AddDeleteDisclaimerMessage_AddsDisclaimer()
        {
            service.AddDeleteDisclaimerMessage();
            AlertMessage disclaimer = service.AlertMessages.First();

            Assert.AreEqual(disclaimer.Message, Messages.ProfileDeleteDisclaimer);
            Assert.AreEqual(disclaimer.Type, AlertMessageType.Danger);
            Assert.AreEqual(disclaimer.Key, String.Empty);
            Assert.AreEqual(disclaimer.FadeOutAfter, 0);
        }

        #endregion

        #region Test helpers

        private void SetUpData()
        {
            account = ObjectFactory.CreateAccount();
            account.Role = ObjectFactory.CreateRole();
            account.RoleId = account.Role.Id;

            context.Set<Account>().Add(account);
            context.SaveChanges();
        }
        private void TearDownData()
        {
            context.Set<Account>().RemoveRange(context.Set<Account>().Where(account => account.Id.StartsWith(ObjectFactory.TestId)));
            context.Set<Role>().RemoveRange(context.Set<Role>().Where(role => role.Id.StartsWith(ObjectFactory.TestId)));
            context.SaveChanges();
        }

        #endregion
    }
}
