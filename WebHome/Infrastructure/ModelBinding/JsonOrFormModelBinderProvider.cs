using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebHome.Infrastructure.ModelBinding
{
    /// <summary>
    /// 為標了 <see cref="FromJsonOrFormAttribute"/> 的參數建立
    /// <see cref="JsonOrFormModelBinder"/>，內含 Body 與值提供者兩組繫結器。
    /// </summary>
    public sealed class JsonOrFormModelBinderProvider : IModelBinderProvider
    {
        // ModelBinderFactory 的快取鍵是 (ModelMetadata, CacheToken)，不含 BindingSource。
        // 兩組子繫結器共用同一份 Metadata，若沿用 context.CreateBinder（token 固定為 Metadata），
        // 第二次建立會直接取回第一次的結果 —— 值提供者那組會變成 BodyModelBinder，
        // 導致 GET／form 請求被 UnsupportedContentTypeFilter 擋成 415。
        private static readonly object BodyBinderCacheToken = new();
        private static readonly object ValueBinderCacheToken = new();

        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (!ReferenceEquals(context.BindingInfo.BindingSource, FromJsonOrFormAttribute.JsonOrFormBindingSource))
            {
                return null;
            }

            var factory = context.Services.GetRequiredService<IModelBinderFactory>();

            // Body 走輸入格式器。
            var bodyBinder = factory.CreateBinder(new ModelBinderFactoryContext
            {
                Metadata = context.Metadata,
                BindingInfo = new BindingInfo(context.BindingInfo) { BindingSource = BindingSource.Body },
                CacheToken = BodyBinderCacheToken,
            });

            // 值提供者（form／query／route）走一般繫結；
            // 這裡必須指定 ModelBinding 而非 null，否則 ModelBinderFactory 會從 Metadata
            // 補回 JsonOrForm 來源，再次選到本 Provider 而無限遞迴。
            var valueBinder = factory.CreateBinder(new ModelBinderFactoryContext
            {
                Metadata = context.Metadata,
                BindingInfo = new BindingInfo(context.BindingInfo) { BindingSource = BindingSource.ModelBinding },
                CacheToken = ValueBinderCacheToken,
            });

            return new JsonOrFormModelBinder(bodyBinder, valueBinder);
        }
    }
}
