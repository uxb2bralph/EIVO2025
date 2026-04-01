using ModelCore.DataEntity;
using System;
namespace ModelCore.UploadManagement
{
    public interface IUploadManager<TUploadItem>
     where TUploadItem : IItemUpload, new()
    {
        System.Collections.Generic.List<TUploadItem> ErrorList { get; }
        bool IsValid { get; }
        int ItemCount { get; }
        System.Collections.Generic.List<TUploadItem> ItemList { get; }
        void ParseData(UserProfile userProfile, string fileName, System.Text.Encoding encoding);
        bool Save();
    }
}
