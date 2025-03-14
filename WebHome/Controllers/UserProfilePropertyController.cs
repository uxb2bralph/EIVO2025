using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelCore.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebHome.Controllers
{
    [Authorize]
    public class UserProfilePropertyController : SampleController<InvoiceItem>
    {
        public UserProfilePropertyController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public ActionResult Edit(int uid, int propertyId)
        {
            var model = new UserProfileProperty
            {
                UID = uid,
                PropertyID = propertyId,
                Property = GetPropertyValue(uid, propertyId)
            };
            return View(model);
        }

        [HttpPost]
        public JsonResult UpdateProperty(int uid, int propertyId, string property)
        {
            UserProfileProperty item = null;
            if (string.IsNullOrEmpty(property))
            {
                DeleteProperty(uid, propertyId);
            }
            else
            {
                item = SaveProperty(uid, propertyId, property);
            }
            return Json(new { result = true, message = item?.ToPlainText() });
        }

        private string GetPropertyValue(int uid, int propertyId)
        {

                var property = db.UserProfileProperty
                    .FirstOrDefault(p => p.UID == uid && p.PropertyID == propertyId);
                return property?.Property;
        }

        private UserProfileProperty SaveProperty(int uid, int propertyId, string property)
        {
            var item = db.UserProfileProperty
                .FirstOrDefault(p => p.UID == uid && p.PropertyID == propertyId);

            if (item != null)
            {
                //existing.Property = property;
                item.ApplyPlainText(property);
            }
            else
            {
                item = new UserProfileProperty
                {
                    UID = uid,
                    PropertyID = propertyId,
                    Property = property
                };
                item.ApplyPlainText(property);
                db.UserProfileProperty.InsertOnSubmit(item);
            }

            db.SubmitChanges();
            return item;
        }

        private void DeleteProperty(int uid, int propertyId)
        {
            var property = db.UserProfileProperty
                .FirstOrDefault(p => p.UID == uid && p.PropertyID == propertyId);

            if (property != null)
            {
                db.UserProfileProperty.DeleteOnSubmit(property);
                db.SubmitChanges();
            }
        }
    }
}