﻿using MvcTemplate.Services;
using MvcTemplate.Validators;
using System;
using System.Web.Mvc;

namespace MvcTemplate.Controllers
{
    public abstract class ValidatedController<TValidator, TService> : ServicedController<TService>
        where TValidator : IValidator
        where TService : IService
    {
        public TValidator Validator { get; private set; }

        protected ValidatedController(TValidator validator, TService service)
            : base(service)
        {
            Validator = validator;
        }

        protected override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            Validator.CurrentAccountId = Service.CurrentAccountId;
            Validator.ModelState = ModelState;
            Validator.Alerts = Alerts;
        }

        protected override void Dispose(Boolean disposing)
        {
            Validator.Dispose();

            base.Dispose(disposing);
        }
    }
}
