using System;
using System.Text;
using ModelCore.DataEntity;
using ModelCore.Security.MembershipManagement;

namespace ModelCore.InvoiceManagement
{
    public interface IGoogleUploadManager : IDisposable
    {
        bool IsValid { get; }
        void ParseData(UserProfile userProfile, string fileName, Encoding encoding);
        bool Save();
        int ItemCount
        {
            get;
        }
    }
}
