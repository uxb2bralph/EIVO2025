using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ModelCore.Locale;
using System.Runtime.Serialization;
using System.Data.Linq;
using CommonLib.DataAccess;

namespace ModelCore.SCMDataEntity
{
    public static partial class ExtensionMethods
    {

    }

    public partial class SCMEntityManager<TEntity> : GenericManager<SCMEntityDataContext, TEntity>
        where TEntity : class,new()
    {
        public SCMEntityManager() : base() { }
        public SCMEntityManager(GenericManager<SCMEntityDataContext> manager) : base(manager) { }
    }

    public delegate TEntity CreateItemFunc<TEntity>(GenericManager<SCMEntityDataContext, TEntity> arg) where TEntity : class,new();

    public partial class PRODUCTS_DATA
    {
        [DataMember()]
        public Naming.DataItemSource DataFrom
        { get; set; }

        [DataMember()]
        public Naming.DataItemSource DataStatus
        { get; set; }

    }

    public partial class PURCHASE_ORDER_RETURNED_DETAILS
    {
        [DataMember()]
        public Naming.DataItemSource DataFrom
        { get; set; }

        [DataMember()]
        public Naming.DataItemStatus DataStatus
        { get; set; }

    }



    public partial class BUYER_ORDERS
    {
        [DataMember()]
        public Naming.DataItemSource DataFrom
        { get; set; }

        public BUYER_ORDERS Clone()
        {
            return (BUYER_ORDERS)this.MemberwiseClone();
        }
    }

    public partial class PURCHASE_ORDER
    {
        [DataMember()]
        public Naming.DataItemSource DataFrom
        { get; set; }
    }

    public partial class SALES_PROMOTION
    {
        [DataMember()]
        public Naming.DataItemSource DataFrom
        { get; set; }

    }

    public partial class WAREHOUSE
    {
        [DataMember()]
        public Naming.DataItemSource DataFrom
        { get; set; }

    }

    public partial class PURCHASE_ORDER_RETURNED
    {
        [DataMember()]
        public Naming.DataItemSource DataFrom
        { get; set; }
    }

    public partial class CDS_Document
    {
        [DataMember()]
        public Naming.DataItemSource DataFrom
        { get; set; }
    }

    public partial class EXCHANGE_GOODS
    {
        [DataMember()]
        public Naming.DataItemSource DataFrom
        { get; set; }
    }

    public partial class WAREHOUSE_WARRANT
    {
        [DataMember()]
        public Naming.DataItemSource DataFrom
        { get; set; }
    }

    public partial class GOODS_RETURNED
    {
        [DataMember()]
        public Naming.DataItemSource DataFrom
        { get; set; }
    }
}



