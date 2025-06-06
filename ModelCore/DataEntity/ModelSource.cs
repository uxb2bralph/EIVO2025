﻿using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using CommonLib.DataAccess;
using ModelCore.Locale;

namespace ModelCore.DataEntity
{
    public class ModelSource<TEntity> : GenericManager<EIVOEntityDataContext, TEntity>
        where TEntity : class, new()
    {
        protected IQueryable<TEntity>? _items;
        protected ModelSourceInquiry<TEntity>? _inquiry;

        public ModelSource() : base() { }
        public ModelSource(GenericManager<EIVOEntityDataContext> manager) : base(manager) { }

        public IQueryable<TEntity> Items
        {
            get
            {
                if (_items == null)
                    _items = this.EntityList;
                return _items;
            }
            set
            {
                _items = value;
            }
        }

        public EIVOEntityDataContext GetDataContext()
        {
            return (EIVOEntityDataContext)this._db;
        }

        public void BuildQuery()
        {
            if (_inquiry != null)
            {
                _inquiry.BuildQueryExpression(this);
            }
        }

        public ModelSourceInquiry<TEntity>? Inquiry
        {
            get
            {
                return _inquiry;
            }
            set
            {
                _inquiry = value;
                _inquiry?.ApplyModelSource(this);
            }
        }

        private void applyModelSource()
        {
            if (_inquiry != null)
            {
                _inquiry.ModelSource = this;
            }
        }

        public String? DataSourcePath
        {
            get;
            set;
        }

        public bool InquiryHasError
        {
            get;
            set;
        }

        public Naming.DataResultMode ResultModel
        {
            get;
            set;
        }

        public int InquiryPageSize
        { get; set; }

        public int InquiryPageIndex
        { get; set; }
    }

    public partial class ModelSourceInquiry<TEntity> : ModelSourceInquiry
        where TEntity : class, new()
    {
        protected List<ModelSourceInquiry<TEntity>>? _chainedInquiry;

        public void ApplyModelSource(ModelSource<TEntity> models)
        {
            this.ModelSource = models;
            if (_chainedInquiry != null)
            {
                foreach (var inquiry in _chainedInquiry)
                {
                    inquiry.ApplyModelSource(models);
                }
            }
        }

        public virtual void BuildQueryExpression(ModelSource<TEntity> models)
        {
            HasError = QueryRequired && !HasSet;
            if (HasError)
            {
                models.Items = models.EntityList.Where(f => false);
                if (ModelSource != null)
                    ModelSource.InquiryHasError = true;
            }
            else
            {
                if (_chainedInquiry != null)
                {
                    foreach (var inquiry in _chainedInquiry)
                    {
                        if (_viewModel != null)
                        {
                            inquiry._viewModel = _viewModel;
                        }
                        inquiry.BuildQueryExpression(models);
                    }
                }
            }
        }

        public ModelSourceInquiry<TEntity> Append(ModelSourceInquiry<TEntity> inquiry)
        {
            if (_chainedInquiry == null)
            {
                _chainedInquiry = new List<ModelSourceInquiry<TEntity>>();
            }
            _chainedInquiry.Add(inquiry);
            return this;
        }

        public ModelSource<TEntity>? ModelSource
        { get; set; }
    }

    public partial class ModelSource : GenericManager<EIVOEntityDataContext>
    {
        public ModelSource() : base() { }
        public ModelSource(GenericManager<EIVOEntityDataContext> manager) : base(manager) { }
    }

    public partial class ModelSourceInquiry
    {

        public bool QueryRequired
        { get; set; }


        public String? AlertMessage
        { get; set; }

        public string? ActionName
        {
            get;
            set;
        }

        public String? ControllerName
        {
            get;
            set;
        }

        public bool HasError
        {
            get;
            protected set;
        }

        public String? QueryMessage
        { get; set; }

        protected bool effective;
        public bool HasSet
        {
            get => effective;
            protected set => effective = value;
        }

        protected object? _viewModel;

    }


}