﻿using MvcTemplate.Resources.Form;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MvcTemplate.Components.Mvc
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class FileSizeAttribute : ValidationAttribute
    {
        public Decimal MaximumMB { get; private set; }

        public FileSizeAttribute(Double maximumMB)
            : base(() => Validations.FileSize)
        {
            MaximumMB = Convert.ToDecimal(maximumMB);
        }

        public override String FormatErrorMessage(String name)
        {
            return String.Format(ErrorMessageString, name, MaximumMB);
        }
        public override Boolean IsValid(Object value)
        {
            IEnumerable<HttpPostedFileBase> files = ToFiles(value);

            if (files == null)
                return true;

            return files.Sum(file => file == null ? 0 : file.ContentLength) <= MaximumMB * 1024 * 1024;
        }

        private IEnumerable<HttpPostedFileBase> ToFiles(Object value)
        {
            HttpPostedFileBase file = value as HttpPostedFileBase;
            if (file != null) return new[] { file };

            return value as IEnumerable<HttpPostedFileBase>;
        }
    }
}
