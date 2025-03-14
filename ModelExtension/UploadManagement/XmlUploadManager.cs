using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

using CommonLib.DataAccess;
using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Security.MembershipManagement;
using Newtonsoft.Json;
using CommonLib.Utility;
using CommonLib.Core.Utility;

namespace ModelCore.UploadManagement
{
    public abstract class XmlUploadManager<T, TEntity> : GenericManager<T, TEntity>, ICsvUploadManager, IUploadManager<ItemUpload<TEntity>>
        where T : DataContext, new()
        where TEntity : class,new()
    {
        protected List<ItemUpload<TEntity>> _items;
        protected List<ItemUpload<TEntity>> _errorItems;
        protected UserProfile _userProfile;
        protected bool _bResult = false;
        protected bool _breakParsing = false;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public XmlUploadManager() : base() {
            initialize();
        }
        public XmlUploadManager(GenericManager<T> manager)
            : base(manager)
        {
            initialize();
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        protected virtual void initialize()
        {
        }


        public virtual void ParseData(UserProfile userProfile, String fileName, Encoding encoding)
        {
            _userProfile = userProfile;
            _bResult = false;

            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            ParseData(doc);
        }

        public virtual void ParseData(XmlDocument doc)
        {
            var items = ((TEntity[])doc.DeserializeDataContract<TEntity[]>()).Select(i => new ItemUpload<TEntity>
            {
                Entity = i,
                UploadStatus = Naming.UploadStatusDefinition.等待匯入
            });

            _items = new List<ItemUpload<TEntity>>();
            _bResult = true;
            _items.AddRange(items);

            int index = 1;
            foreach (var item in _items)
            {
                if (!validate(item))
                {
                    item.Status = String.Format("第{0}筆:{1}", index, item.Status);
                    _bResult = false;
                }
                index++;
            }

            if (!IsValid)
            {
                _errorItems = _items.Where(i => !String.IsNullOrEmpty(i.Status)).ToList();
            }

        }

        public virtual void SaveData(XmlDocument doc)
        {
            var items = ((TEntity[])doc.DeserializeDataContract<TEntity[]>()).Select(i => new ItemUpload<TEntity>
            {
                Entity = i,
                UploadStatus = Naming.UploadStatusDefinition.等待匯入
            });

            _items = new List<ItemUpload<TEntity>>();
            _bResult = true;
            _items.AddRange(items);
            _errorItems = new List<ItemUpload<TEntity>>();

            int index = 1;
            foreach (var item in _items)
            {
                try
                {
                    if (validate(item))
                    {
                        this.SubmitChanges();
                    }
                    else
                    {
                        item.Status = String.Format("第{0}筆:{1}", index, item.Status);
                        item.DataContent = JsonConvert.SerializeObject(item.Entity);
                        _bResult = false;
                        _errorItems.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    FileLogger.Logger.Error(ex);
                    item.Status = String.Format("第{0}筆:{1}", index, ex);
                    item.DataContent = JsonConvert.SerializeObject(item.Entity);
                    _bResult = false;
                    _errorItems.Add(item);
                }
                index++;
            }

            finalProcess();

        }

        public bool IsValid
        {
            get
            {
                return _bResult;
            }
        }

        public List<ItemUpload<TEntity>> ItemList
        {
            get
            {
                return _items;
            }
        }

        public List<ItemUpload<TEntity>> ErrorList
        {
            get
            {
                return _errorItems;
            }
        }


        public int ItemCount
        {
            get
            {
                return _items.Count;
            }
        }

        protected abstract void doSave();

        public bool Save()
        {
            if (_bResult)
            {
                doSave();
                foreach (var item in _items)
                {
                    item.UploadStatus = Naming.UploadStatusDefinition.匯入成功;
                }
                return true;
            }
            return false;
        }

        protected abstract bool validate(ItemUpload<TEntity> item);

        protected virtual void finalProcess()
        { }


    }

}
