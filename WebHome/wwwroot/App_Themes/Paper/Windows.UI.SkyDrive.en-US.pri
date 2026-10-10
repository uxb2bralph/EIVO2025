      </remarks>
        </member>
        <member name="P:Microsoft.Data.OData.ODataMessageReaderSettings.DisableMessageStreamDisposal">
            <summary>Gets or sets a value that indicates whether the message stream will not be disposed after finishing writing with the message.</summary>
            <returns>true if the message stream will not be disposed after finishing writing with the message; otherwise false. The default value is false.</returns>
        </member>
        <member name="P:Microsoft.Data.OData.ODataMessageReaderSettings.MaxProtocolVersion">
            <summary>Gets or sets the maximum OData protocol version the reader should accept and understand.</summary>
            <returns>The maximum OData protocol version the reader should accept and understand.</returns>
            <remarks>
            If the payload to be read has higher DataServiceVersion than the value specified for this property
            the reader will fail.
            Reader will also not report features which require higher version than specified for this property.
            It may either ignore such features in the payload or fail on them.
            </remarks>
        </member>
        <member name="P:Microsoft.Data.OData.ODataMessageReaderSettings.DisableStrictMetadataValidation">
            <summary>
            false - metadata validation is strict, the input must exactly match against the model.
            true - metadata validation is lax, the input doesn't have to match the model in all cases.
            This property has effect only if the metadata model is specified.
            </summary>
            <remarks>
            Strict metadata validation:
              Primitive values: The wire type must be convertible to the expected type.
              Complex values: The wire type must resolve against the model and it must exactly match the expected type.
              Entities: The wire type must resolve against the model and it must be assignable to the expected type.
              Collections: The wire type must exactly match the expected type.
              If no expected type is available we use the payload type.
            Lax metadata validation:
              Primitive values: If expected type is available, we ignore the wire type.
              Complex values: The wire type is used if the model defines it. If the model doesn't define such a type, the expected type is used.
                If the wire type is not equal to the expected type, but it's assignable, we fail because we don't support complex type inheritance.
                If the wire type if not assignable we use the expected type.
              Entities: same as complex values except that if the payload type is assignable we use the payload type. This allows derived entity types.
              Collections: If expected type is available, we ignore the wire type, except we fail if the item type is a derived complex type.
              If no expected type is available we use the payload type and it must resolve against the model.
            If DisablePrimitiveTypeConversion is on, the rules for primitive values don't apply
              and the primitive values are always read with the type from the wire.
            </remarks>
        </member>
        <member name="P:Microsoft.Data.OData.ODataMessageReaderSettings.ReaderBehavior">
            <summary>
            The reader behavior that holds all the knobs needed to make the reader
            behave differently inside and outside of WCF Data Services.
            </summary>
        </member>
        <member name="P:Microsoft.Data.OData.ODataMessageReaderSettings.AtomEntryXmlCustomizationCallback">
            <summary>
            ATOM entry XML customization callback.
            </summary>
        </member>
        <member name="P:Microsoft.Data.OData.ODataMessageReaderSettings.ReportUndeclaredLinkProperties">
            <summary>
            Whether or not to report any undeclared link properties in the payload. Computed from the UndeclaredPropertyBehaviorKinds enum property.
            </summary>
        </member>
        <member name="P:Microsoft.Data.OData.ODataMessageReaderSettings.IgnoreUndeclaredValueProperties">
            <summary>
            Whether or not to ignore any undeclared value properties in the payload. Computed from the UndeclaredPropertyBehaviorKinds enum property.
            </summary>
        </member>
        <member name="T:Microsoft.Data.OData.ODataMessageReader">
            <summary>
            Reader class used to read all OData payloads (entries, feeds, metadata documents, service documents, etc.).
            </summary>
        </member>
        <member name="F:Microsoft.Data.OData.ODataMessageReader.message">
            <summary>The message for which the message reader was created.</summary>
        </member>
        <member name="F:Microsoft.Data.OData.ODataMessageReader.readingResponse">
            <summary>A flag indicating whether we are reading a request or a response message.</summary>
        </member>
        <member name="F:Microsoft.Data.OData.ODataMessageReader.settings">
            <summary>The message reader settings to use when reading the message payload.</summary>
        </member>
        <member name="F:Microsoft.Data.OData.ODataMessageReader.model">
            <summary>The model. Non-null if we do have metadata available.</summary>
        </member>
        <member name="F:Microsoft.Data.OData.ODataMessageReader.version">
            <summary>The <see cref="T:Microsoft.Data.OData.ODataVersion"/> to be used for reading the payload.</summary>
        </member>
        <member name="F:Microsoft.Data.OData.ODataMessageReader.urlResolver">
            <summary>The optional URL resolver to perform custom URL resolution for URLs read from the payload.</summary>
        </member>
        <member name="F:Microsoft.Data.OData.ODataMessageReader.edmTypeResolver">
            <summary>The resolver to use when determining an entity set's element type.</summary>
        </member>
        <member name="F:Microsoft.Data.OData.ODataMessageReader.readMethodCalled">
            <summary>Flag to ensure that only a single read method is called on the message reader.</summary>
        </member>
        <member name="F:Microsoft.Data.OData.ODataMessageReader.isDisposed">
            <summary>true if Dispose() has been called on this message reader, false otherwise.</summary>
        </member>
        <member name="F:Microsoft.Data.OData.ODataMessageReader.inputContext">
            <summary>The input context used to read the message content.</summary>
        </member>
        <member name="F:Microsoft.Data.OData.ODataMessageReader.readerPayloadKind">
            <summary>The payload kind of the payload to be read with this reader.</summary>
            <remarks>This field is set implicitly when one of the read (or reader creation) methods is called.</remarks>
        </member>
        <member name="F:Microsoft.Data.OData.ODataMessageReader.format">
            <summary>The <see cref="T:Microsoft.Data.OData.ODataFormat"/> of the payload to be read with this reader.</summary>
            <remarks>This field is set implicitly when one of the read (or reader creation) methods is called.</remarks>
        </member>
        <member name="F:Microsoft.Data.OData.ODataMessageReader.contentType">
            <summary>The <see cref="T:Microsoft.Data.OData.MediaType"/> parsed from the content type header.</summary>
            <remarks>This field is set implicitly when one of the read (or reader creation) methods is called.</remarks>
        </member>
        <member name="F:Microsoft.Data.OData.ODataMessageReader.encoding">
            <summary>The <see cref="T:System.Text.Encoding"/> of the payload to be read with this reader.</summary>
            <remarks>This field is set implicitly when one of the read (or reader creation) methods is called.</remarks>
        </member>
        <member name="F:Microsoft.Data.OData.ODataMessageReader.batchBoundary">
            <summary>The batch boundary string if the payload to be read is a batch request or response.</summary>
            <remarks>This is set implicitly when the CreateBatchReader method is called.</remarks>
        </member>
        <member name="F:Microsoft.Data.OData.ODataMessageReader.mediaTypeResolver">
            <summary>The media type resolver to use when interpreting the incoming content type.</summary>
        </member>
        <member name="F:Microsoft.Data.OData.ODataMessageReader.payloadKindDetectionFormatStates">
            <summary>Storage for format specific states from payload kind detection.</summary>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.#ctor(Microsoft.Data.OData.IODataRequestMessage)">
            <summary>Creates a new <see cref="T:Microsoft.Data.OData.ODataMessageReader" /> for the given request message.</summary>
            <param name="requestMessage">The request message for which to create the reader.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.#ctor(Microsoft.Data.OData.IODataRequestMessage,Microsoft.Data.OData.ODataMessageReaderSettings)">
            <summary>Creates a new <see cref="T:Microsoft.Data.OData.ODataMessageReader" /> for the given request message and message reader settings.</summary>
            <param name="requestMessage">The request message for which to create the reader.</param>
            <param name="settings">The message reader settings to use for reading the message payload.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.#ctor(Microsoft.Data.OData.IODataRequestMessage,Microsoft.Data.OData.ODataMessageReaderSettings,Microsoft.Data.Edm.IEdmModel)">
            <summary>
            Creates a new ODataMessageReader for the given request message and message reader settings.
            </summary>
            <param name="requestMessage">The request message for which to create the reader.</param>
            <param name="settings">The message reader settings to use for reading the message payload.</param>
            <param name="model">The model to use.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.#ctor(Microsoft.Data.OData.IODataResponseMessage)">
            <summary>Creates a new <see cref="T:System.Data.OData.ODataMessageReader" /> for the given response message.</summary>
            <param name="responseMessage">The response message for which to create the reader.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.#ctor(Microsoft.Data.OData.IODataResponseMessage,Microsoft.Data.OData.ODataMessageReaderSettings)">
            <summary>Creates a new <see cref="T:Microsoft.Data.OData.ODataMessageReader" /> for the given response message and message reader settings.</summary>
            <param name="responseMessage">The response message for which to create the reader.</param>
            <param name="settings">The message reader settings to use for reading the message payload.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.#ctor(Microsoft.Data.OData.IODataResponseMessage,Microsoft.Data.OData.ODataMessageReaderSettings,Microsoft.Data.Edm.IEdmModel)">
            <summary>
            Creates a new ODataMessageReader for the given response message and message reader settings.
            </summary>
            <param name="responseMessage">The response message for which to create the reader.</param>
            <param name="settings">The message reader settings to use for reading the message payload.</param>
            <param name="model">The model to use.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.DetectPayloadKind">
            <summary>Determines the potential payload kinds and formats of the payload being read and returns it.</summary>
            <returns>The set of potential payload kinds and formats for the payload being read by this reader.</returns>
            <remarks>When this method is called it first analyzes the content type and determines whether there
            are multiple matching payload kinds registered for the message's content type. If there are, it then
            runs the payload kind detection on all formats that have a matching payload kind registered.
            Note that this method can return multiple results if a payload is valid for multiple payload kinds but
            will always at most return a single result per payload kind.
            </remarks>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.DetectPayloadKindAsync">
            <summary>Determines the potential payload kinds and formats of the payload being read and returns it.</summary>
            <returns>The set of potential payload kinds and formats for the payload being read by this reader.</returns>
            <remarks>When this method is called it first analyzes the content type and determines whether there
            are multiple matching payload kinds registered for the message's content type. If there are, it then
            runs the payload kind detection on all formats that have a matching payload kind registered.
            Note that this method can return multiple results if a payload is valid for multiple payload kinds but
            will always at most return a single result per payload kind.
            </remarks>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.CreateODataFeedReader">
            <summary>Creates an <see cref="T:Microsoft.Data.OData.ODataReader" /> to read a feed.</summary>
            <returns>The created reader.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.CreateODataFeedReader(Microsoft.Data.Edm.IEdmEntityType)">
            <summary>
            Creates an <see cref="T:Microsoft.Data.OData.ODataReader"/> to read a feed.
            </summary>
            <param name="expectedBaseEntityType">The expected base type for the entities in the feed.</param>
            <returns>The created reader.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.CreateODataFeedReader(Microsoft.Data.Edm.IEdmEntitySet,Microsoft.Data.Edm.IEdmEntityType)">
            <summary>
            Creates an <see cref="T:Microsoft.Data.OData.ODataReader"/> to read a feed.
            </summary>
            <param name="entitySet">The entity set we are going to read entities for.</param>
            <param name="expectedBaseEntityType">The expected base type for the entities in the feed.</param>
            <returns>The created reader.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.CreateODataFeedReaderAsync">
            <summary>Asynchronously creates an <see cref="T:Microsoft.Data.OData.ODataReader" /> to read a feed.</summary>
            <returns>A running task for the created reader.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.CreateODataFeedReaderAsync(Microsoft.Data.Edm.IEdmEntityType)">
            <summary>
            Asynchronously creates an <see cref="T:Microsoft.Data.OData.ODataReader"/> to read a feed.
            </summary>
            <param name="expectedBaseEntityType">The expected base type for the entities in the feed.</param>
            <returns>A running task for the created reader.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.CreateODataFeedReaderAsync(Microsoft.Data.Edm.IEdmEntitySet,Microsoft.Data.Edm.IEdmEntityType)">
            <summary>
            Asynchronously creates an <see cref="T:Microsoft.Data.OData.ODataReader"/> to read a feed.
            </summary>
            <param name="entitySet">The entity set we are going to read entities for.</param>
            <param name="expectedBaseEntityType">The expected base type for the entities in the feed.</param>
            <returns>A running task for the created reader.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.CreateODataEntryReader">
            <summary>Creates an <see cref="T:Microsoft.Data.OData.ODataReader" /> to read an entry.</summary>
            <returns>The created reader.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.CreateODataEntryReader(Microsoft.Data.Edm.IEdmEntityType)">
            <summary>
            Creates an <see cref="T:Microsoft.Data.OData.ODataReader"/> to read an entry.
            </summary>
            <param name="entityType">The expected entity type for the entry to be read.</param>
            <returns>The created reader.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.CreateODataEntryReader(Microsoft.Data.Edm.IEdmEntitySet,Microsoft.Data.Edm.IEdmEntityType)">
            <summary>
            Creates an <see cref="T:Microsoft.Data.OData.ODataReader"/> to read an entry.
            </summary>
            <param name="entitySet">The entity set we are going to read entities for.</param>
            <param name="entityType">The expected entity type for the entry to be read.</param>
            <returns>The created reader.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.CreateODataEntryReaderAsync">
            <summary>Asynchronously creates an <see cref="T:System.Data.OData.ODataReader" /> to read an entry.</summary>
            <returns>A running task for the created reader.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.CreateODataEntryReaderAsync(Microsoft.Data.Edm.IEdmEntityType)">
            <summary>
            Asynchronously creates an <see cref="T:Microsoft.Data.OData.ODataReader"/> to read an entry.
            </summary>
            <param name="entityType">The expected entity type for the entry to be read.</param>
            <returns>A running task for the created reader.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.CreateODataEntryReaderAsync(Microsoft.Data.Edm.IEdmEntitySet,Microsoft.Data.Edm.IEdmEntityType)">
            <summary>
            Asynchronously creates an <see cref="T:Microsoft.Data.OData.ODataReader"/> to read an entry.
            </summary>
            <param name="entitySet">The entity set we are going to read entities for.</param>
            <param name="entityType">The expected entity type for the entry to be read.</param>
            <returns>A running task for the created reader.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.CreateODataCollectionReader">
            <summary>Creates an <see cref="T:Microsoft.Data.OData.ODataCollectionReader" /> to read a collection of primitive or complex values (as result of a service operation invocation).</summary>
            <returns>The created collection reader.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.CreateODataCollectionReader(Microsoft.Data.Edm.IEdmTypeReference)">
            <summary>
            Creates an <see cref="T:Microsoft.Data.OData.ODataCollectionReader"/> to read a collection of primitive or complex values (as result of a service operation invocation).
            </summary>
            <param name="expectedItemTypeReference">The expected type reference for the items in the collection.</param>
            <returns>The created collection reader.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.CreateODataCollectionReaderAsync">
            <summary>Asynchronously creates an <see cref="T:Microsoft.Data.OData.ODataCollectionReader" /> to read a collection of primitive or complex values (as result of a service operation invocation).</summary>
            <returns>A running task for the created collection reader.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.CreateODataCollectionReaderAsync(Microsoft.Data.Edm.IEdmTypeReference)">
            <summary>
            Asynchronously creates an <see cref="T:Microsoft.Data.OData.ODataCollectionReader"/> to read a collection of primitive or complex values (as result of a service operation invocation).
            </summary>
            <param name="expectedItemTypeReference">The expected type reference for the items in the collection.</param>
            <returns>A running task for the created collection reader.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.CreateODataBatchReader">
            <summary>Creates an <see cref="T:Microsoft.Data.OData.ODataBatchReader" /> to read a batch of requests or responses.</summary>
            <returns>The created batch reader.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.CreateODataBatchReaderAsync">
            <summary>Asynchronously creates an <see cref="T:Microsoft.Data.OData.ODataBatchReader" /> to read a batch of requests or responses.</summary>
            <returns>A running task for the created batch reader.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.CreateODataParameterReader(Microsoft.Data.Edm.IEdmFunctionImport)">
            <summary>
            Creates an <see cref="T:Microsoft.Data.OData.ODataParameterReader"/> to read the parameters for <paramref name="functionImport"/>.
            </summary>
            <param name="functionImport">The function import whose parameters are being read.</param>
            <returns>The created parameter reader.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.CreateODataParameterReaderAsync(Microsoft.Data.Edm.IEdmFunctionImport)">
            <summary>
            Asynchronously creates an <see cref="T:Microsoft.Data.OData.ODataParameterReader"/> to read the parameters for <paramref name="functionImport"/>.
            </summary>
            <param name="functionImport">The function import whose parameters are being read.</param>
            <returns>A running task for the created parameter reader.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.ReadServiceDocument">
            <summary>Reads a service document payload.</summary>
            <returns>The service document read.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.ReadServiceDocumentAsync">
            <summary>Asynchronously reads a service document payload.</summary>
            <returns>A task representing the asynchronous operation of reading the service document.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.ReadProperty">
            <summary>Reads an <see cref="T:Microsoft.Data.OData.ODataProperty" /> as message payload.</summary>
            <returns>The property read from the payload.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.ReadProperty(Microsoft.Data.Edm.IEdmTypeReference)">
            <summary>
            Reads an <see cref="T:Microsoft.Data.OData.ODataProperty"/> as message payload.
            </summary>
            <param name="expectedPropertyTypeReference">The expected type reference of the property to read.</param>
            <returns>The property read from the payload.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.ReadProperty(Microsoft.Data.Edm.IEdmStructuralProperty)">
            <summary>
            Reads an <see cref="T:Microsoft.Data.OData.ODataProperty"/> as message payload.
            </summary>
            <param name="property">The metadata of the property to read.</param>
            <returns>The property read from the payload.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.ReadPropertyAsync">
            <summary>Asynchronously reads an <see cref="T:Microsoft.Data.OData.ODataProperty" /> as message payload.</summary>
            <returns>A task representing the asynchronous operation of reading the property.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.ReadPropertyAsync(Microsoft.Data.Edm.IEdmTypeReference)">
            <summary>
            Asynchronously reads an <see cref="T:Microsoft.Data.OData.ODataProperty"/> as message payload.
            </summary>
            <param name="expectedPropertyTypeReference">The expected type reference of the property to read.</param>
            <returns>A task representing the asynchronous operation of reading the property.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.ReadPropertyAsync(Microsoft.Data.Edm.IEdmStructuralProperty)">
            <summary>
            Asynchronously reads an <see cref="T:Microsoft.Data.OData.ODataProperty"/> as message payload.
            </summary>
            <param name="property">The metadata of the property to read.</param>
            <returns>A task representing the asynchronous operation of reading the property.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.ReadError">
            <summary>Reads an <see cref="T:Microsoft.Data.OData.ODataError" /> as the message payload.</summary>
            <returns>The <see cref="T:Microsoft.Data.OData.ODataError" /> read from the message payload.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.ReadErrorAsync">
            <summary>Asynchronously reads an <see cref="T:Microsoft.Data.OData.ODataError" /> as the message payload.</summary>
            <returns>A task representing the asynchronous operation of reading the error.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.ReadEntityReferenceLinks">
            <summary>Reads the result of a $links query (entity reference links) as the message payload.</summary>
            <returns>The entity reference links read as message payload.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.ReadEntityReferenceLinks(Microsoft.Data.Edm.IEdmNavigationProperty)">
            <summary>
            Reads the result of a $links query (entity reference links) as the message payload.
            </summary>
            <param name="navigationProperty">The navigation property for which to read the entity reference links.</param>
            <returns>The entity reference links read as message payload.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.ReadEntityReferenceLinksAsync">
            <summary>Asynchronously reads the result of a $links query as the message payload.</summary>
            <returns>A task representing the asynchronous reading of the entity reference links.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.ReadEntityReferenceLinksAsync(Microsoft.Data.Edm.IEdmNavigationProperty)">
            <summary>
            Asynchronously reads the result of a $links query as the message payload.
            </summary>
            <param name="navigationProperty">The navigation property for which to read the entity reference links.</param>
            <returns>A task representing the asynchronous reading of the entity reference links.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.ReadEntityReferenceLink">
            <summary>Reads a singleton result of a $links query (entity reference link) as the message payload.</summary>
            <returns>The entity reference link read from the message payload.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.ReadEntityReferenceLink(Microsoft.Data.Edm.IEdmNavigationProperty)">
            <summary>
            Reads a singleton result of a $links query (entity reference link) as the message payload.
            </summary>
            <param name="navigationProperty">The navigation property for which to read the entity reference link.</param>
            <returns>The entity reference link read from the message payload.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.ReadEntityReferenceLinkAsync">
            <summary>Asynchronously reads a singleton result of a $links query (entity reference link) as the message payload.</summary>
            <returns>A running task representing the reading of the entity reference link.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.ReadEntityReferenceLinkAsync(Microsoft.Data.Edm.IEdmNavigationProperty)">
            <summary>
            Asynchronously reads a singleton result of a $links query (entity reference link) as the message payload.
            </summary>
            <param name="navigationProperty">The navigation property for which to read the entity reference link.</param>
            <returns>A running task representing the reading of the entity reference link.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.ReadValue(Microsoft.Data.Edm.IEdmTypeReference)">
            <summary>
            Reads a single value as the message body.
            </summary>
            <param name="expectedTypeReference">The expected type reference for the value to be read; null if no expected type is available.</param>
            <returns>The read value.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.ReadValueAsync(Microsoft.Data.Edm.IEdmTypeReference)">
            <summary>
            Asynchronously reads a single value as the message body.
            </summary>
            <param name="expectedTypeReference">The expected type reference for the value to be read; null if no expected type is available.</param>
            <returns>A running task representing the reading of the value.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.ReadMetadataDocument">
            <summary>Reads the message body as metadata document.</summary>
            <returns>Returns <see cref="T:Microsoft.Data.Edm.IEdmModel" />.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.Dispose">
            <summary><see cref="M:System.IDisposable.Dispose()" /> implementation to cleanup unmanaged resources of the reader. </summary>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.GetFormat">
            <summary>
            Determines the format of the payload being read and returns it.
            </summary>
            <returns>The format of the payload being read by this reader.</returns>
            <remarks>
            The format of the payload is determined when starting to read the message; 
            if this method is called before reading has started it will throw.
            </remarks>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.ProcessContentType(Microsoft.Data.OData.ODataPayloadKind[])">
            <summary>
            Processes the content type header of the message to determine the format of the payload, the encoding, and the payload kind.
            </summary>
            <param name="payloadKinds">All possible kinds of payload to be read with this message reader; must not include ODataPayloadKind.Unsupported.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.GetContentTypeHeader">
            <summary>
            Gets the content type header of the message and validates that it is present and not empty.
            </summary>
            <returns>The content type header of the message.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.VerifyCanCreateODataFeedReader(Microsoft.Data.Edm.IEdmEntitySet,Microsoft.Data.Edm.IEdmEntityType)">
            <summary>
            Verify arguments for creation of an <see cref="T:Microsoft.Data.OData.ODataReader"/> to read a feed.
            </summary>
            <param name="entitySet">The entity set we are going to read entities for.</param>
            <param name="expectedBaseEntityType">The expected base entity type for the entities in the feed.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.VerifyCanCreateODataEntryReader(Microsoft.Data.Edm.IEdmEntitySet,Microsoft.Data.Edm.IEdmEntityType)">
            <summary>
            Verify arguments for creation of an <see cref="T:Microsoft.Data.OData.ODataReader"/> to read an entry.
            </summary>
            <param name="entitySet">The entity set we are going to read entities for.</param>
            <param name="entityType">The expected entity type for the entry to be read.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.VerifyCanCreateODataCollectionReader(Microsoft.Data.Edm.IEdmTypeReference)">
            <summary>
            Verify arguments for creation of an <see cref="T:Microsoft.Data.OData.ODataCollectionReader"/> to read a collection of primitive or complex values 
            (as result of a service operation invocation).
            </summary>
            <param name="expectedItemTypeReference">The expected type for the items in the collection.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.VerifyCanCreateODataBatchReader">
            <summary>
            Verify arguments for creation of a batch as the message body.
            </summary>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.VerifyCanCreateODataParameterReader(Microsoft.Data.Edm.IEdmFunctionImport)">
            <summary>
            Verify arguments for creation of an <see cref="T:Microsoft.Data.OData.ODataParameterReader"/> to read the parameters for <paramref name="functionImport"/>.
            </summary>
            <param name="functionImport">The function import whose parameters are being read.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.VerifyCanReadServiceDocument">
            <summary>
            Verify arguments for reading of a service document payload.
            </summary>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.VerifyCanReadMetadataDocument">
            <summary>
            Verify arguments for reading of a metadata document payload.
            </summary>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.VerifyCanReadProperty(Microsoft.Data.Edm.IEdmStructuralProperty)">
            <summary>
            Verify arguments for reading of an <see cref="T:Microsoft.Data.OData.ODataProperty"/> as message payload.
            </summary>
            <param name="property">The metadata of the property to read.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.VerifyCanReadProperty(Microsoft.Data.Edm.IEdmTypeReference)">
            <summary>
            Verify arguments for reading of an <see cref="T:Microsoft.Data.OData.ODataProperty"/> as message payload.
            </summary>
            <param name="expectedPropertyTypeReference">The expected type reference of the property to read.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.VerifyCanReadError">
            <summary>
            Verify arguments for reading of an <see cref="T:Microsoft.Data.OData.ODataError"/> as the message payload.
            </summary>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.VerifyCanReadEntityReferenceLinks(Microsoft.Data.Edm.IEdmNavigationProperty)">
            <summary>
            Verify arguments for reading of the result of a $links query (entity reference links) as the message payload.
            </summary>
            <param name="navigationProperty">The navigation property for which to read the entity reference links.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.VerifyCanReadEntityReferenceLink">
            <summary>
            Verify arguments for reading of a singleton result of a $links query (entity reference link) as the message payload.
            </summary>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.VerifyCanReadValue(Microsoft.Data.Edm.IEdmTypeReference)">
            <summary>
            Verify arguments for reading of a single value as the message body.
            </summary>
            <param name="expectedTypeReference">The expected type reference for the value to be read; null if no expected type is available.</param>
            <returns>The payload kinds allowed for the given expected type.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.VerifyReaderNotDisposedAndNotUsed">
            <summary>
            Verifies that the ODataMessageReader has not been used before; an ODataMessageReader can only be used to
            read a single message payload but cannot be reused later.
            </summary>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.VerifyNotDisposed">
            <summary>
            Check if the object has been disposed. Throws an ObjectDisposedException if the object has already been disposed.
            </summary>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.Dispose(System.Boolean)">
            <summary>
            Perform the actual cleanup work.
            </summary>
            <param name="disposing">If 'true' this method is called from user code; if 'false' it is called by the runtime.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.ReadFromInput``1(System.Func{Microsoft.Data.OData.ODataInputContext,``0},Microsoft.Data.OData.ODataPayloadKind[])">
            <summary>
            Method which creates an input context around the input message and calls a func to read the input.
            </summary>
            <typeparam name="T">The type returned by the read method.</typeparam>
            <param name="readFunc">The read function which will be called over the created input context.</param>
            <param name="payloadKinds">All possible kinds of payload to read.</param>
            <returns>The read value from the input.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.TryGetSinglePayloadKindResultFromContentType(System.Collections.Generic.IEnumerable{Microsoft.Data.OData.ODataPayloadKindDetectionResult}@)">
            <summary>
            Gets all the supported payload kinds for a given content type across all formats and returns them.
            </summary>
            <param name="payloadKindResults">The set of supported payload kinds for the content type of the message.</param>
            <returns>true if no or a single payload kind was found for the content type; false if more than one payload kind was found.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.ComparePayloadKindDetectionResult(Microsoft.Data.OData.ODataPayloadKindDetectionResult,Microsoft.Data.OData.ODataPayloadKindDetectionResult)">
            <summary>
            Compares two payload kind detection results.
            </summary>
            <param name="first">The first <see cref="T:Microsoft.Data.OData.ODataPayloadKindDetectionResult"/>.</param>
            <param name="second">The second <see cref="T:Microsoft.Data.OData.ODataPayloadKindDetectionResult"/>.</param>
            <returns>-1 if <paramref name="first"/> is considered less than <paramref name="second"/>,
            0 if the kinds are considered equal, 1 if <paramref name="first"/> is considered greater than <paramref name="second"/>.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.GetPayloadKindDetectionTasks(System.Collections.Generic.IEnumerable{Microsoft.Data.OData.ODataPayloadKindDetectionResult},System.Collections.Generic.List{Microsoft.Data.OData.ODataPayloadKindDetectionResult})">
            <summary>
            Get an enumerable of tasks to get the supported payload kinds for all formats.
            </summary>
            <param name="payloadKindsFromContentType">All payload kinds for which we found matches in some format based on the content type.</param>
            <param name="detectionResults">The list of combined detection results after sniffing.</param>
            <returns>A lazy enumerable of tasks to get the supported payload kinds for all formats.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataMessageReader.ReadFromInputAsync``1(System.Func{Microsoft.Data.OData.ODataInputContext,System.Threading.Tasks.Task{``0}},Microsoft.Data.OData.ODataPayloadKind[])">
            <summary>
            Method which asynchronously creates an input context around the input message and calls a func to read the input.
            </summary>
            <typeparam name="T">The type returned by the read method.</typeparam>
            <param name="readFunc">The read function which will be called over the created input context.</param>
            <param name="payloadKinds">All possible kinds of payload to read.</param>
            <returns>A task which when completed return the read value from the input.</returns>
        </member>
        <member name="P:Microsoft.Data.OData.ODataMessageReader.Settings">
            <summary>
            The message reader settings to use when reading the message payload.
            </summary>
        </member>
        <member name="P:Microsoft.Data.OData.ODataMessageReader.MediaTypeResolver">
            <summary>
            The media type resolver to use when interpreting the incoming content type.
            </summary>
        </member>
        <member name="T:Microsoft.Data.OData.Json.JsonNodeType">
            <summary>
            Enumeration of all JSON node type.
            </summary>
        </member>
        <member name="F:Microsoft.Data.OData.Json.JsonNodeType.None">
            <summary>
            No node - invalid value.
            </summary>
        </member>
        <member name="F:Microsoft.Data.OData.Json.JsonNodeType.StartObject">
            <summary>
            Start of JSON object record, the { character.
            </summary>
        </member>
        <member name="F:Microsoft.Data.OData.Json.JsonNodeType.EndObject">
            <summary>
            End of JSON object record, the } character.
            </summary>
        </member>
        <member name="F:Microsoft.Data.OData.Json.JsonNodeType.StartArray">
            <summary>
            Start of JSON array, the [ character.
            </summary>
        </member>
        <member name="F:Microsoft.Data.OData.Json.JsonNodeType.EndArray">
            <summary>
            End of JSON array, the ] character.
            </summary>
        </member>
        <member name="F:Microsoft.Data.OData.Json.JsonNodeType.Property">
            <summary>
            Property, the name of the property (the value will be reported as a separate node or nodes)
            </summary>
        </member>
        <member name="F:Microsoft.Data.OData.Json.JsonNodeType.PrimitiveValue">
            <summary>
            Primitive value, that is either null, true, false, number or string.
            </summary>
        </member>
        <member name="F:Microsoft.Data.OData.Json.JsonNodeType.EndOfInput">
            <summary>
            End of input reached.
            </summary>
        </member>
        <member name="T:Microsoft.Data.OData.ODataUtils">
            <summary>
            Utility methods used with the OData library.
            </summary>
        </member>
        <member name="F:Microsoft.Data.OData.ODataUtils.Version1NumberString">
            <summary>String representation of the version 1.0 of the OData protocol.</summary>
        </member>
        <member name="F:Microsoft.Data.OData.ODataUtils.Version2NumberString">
            <summary>String representation of the version 2.0 of the OData protocol.</summary>
        </member>
        <member name="F:Microsoft.Data.OData.ODataUtils.Version3NumberString">
            <summary>String representation of the version 3.0 of the OData protocol.</summary>
        </member>
        <member name="M:Microsoft.Data.OData.ODataUtils.SetHeadersForPayload(Microsoft.Data.OData.ODataMessageWriter,Microsoft.Data.OData.ODataPayloadKind)">
            <summary>Sets the content-type and data service version headers on the message used by the message writer.</summary>
            <returns>The content-type and data service version headers on the message used by the message writer.</returns>
            <param name="messageWriter">The message writer to set the headers for.</param>
            <param name="payloadKind">The kind of payload to be written with the message writer.</param>
            <remarks>
            This method can be called if it is important to set all the message headers before calling any of the
            write methods on the <paramref name="messageWriter"/>.
            If it is sufficient to set the headers when the write methods on the <paramref name="messageWriter"/> 
            are called, you don't have to call this method and setting the headers will happen automatically.
            </remarks>
        </member>
        <member name="M:Microsoft.Data.OData.ODataUtils.GetReadFormat(Microsoft.Data.OData.ODataMessageReader)">
            <summary>Returns the format used by the message reader for reading the payload.</summary>
            <returns>The format used by the messageReader for reading the payload.</returns>
            <param name="messageReader">The <see cref="T:Microsoft.Data.OData.ODataMessageReader" /> to get the read format from.</param>
            <remarks>This method must only be called once reading has started.
            This means that a read method has been called on the <paramref name="messageReader"/> or that a reader (for entries, feeds, collections, etc.) has been created.
            If the method is called prior to that it will throw.</remarks>
        </member>
        <member name="M:Microsoft.Data.OData.ODataUtils.LoadODataAnnotations(Microsoft.Data.Edm.IEdmModel)">
            <summary>
            Loads the supported, OData-specific serializable annotations into their in-memory representations.
            </summary>
            <param name="model">The <see cref="T:Microsoft.Data.Edm.IEdmModel"/> to process.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ODataUtils.LoadODataAnnotations(Microsoft.Data.Edm.IEdmModel,System.Int32)">
            <summary>
            Loads the supported, OData-specific serializable annotations into their in-memory representations.
            </summary>
            <param name="model">The <see cref="T:Microsoft.Data.Edm.IEdmModel"/> to process.</param>
            <param name="maxEntityPropertyMappingsPerType">The maximum number of entity mapping attributes to be found 
            for an entity type (on the type itself and all its base types).</param>
        </member>
        <member name="M:Microsoft.Data.OData.ODataUtils.LoadODataAnnotations(Microsoft.Data.Edm.IEdmModel,Microsoft.Data.Edm.IEdmEntityType)">
            <summary>
            Loads the supported, OData-specific serializable annotations into their in-memory representations.
            </summary>
            <param name="model">The <see cref="T:Microsoft.Data.Edm.IEdmModel"/> containing the annotations.</param>
            <param name="entityType">The <see cref="T:Microsoft.Data.Edm.IEdmEntityType"/> to process.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ODataUtils.LoadODataAnnotations(Microsoft.Data.Edm.IEdmModel,Microsoft.Data.Edm.IEdmEntityType,System.Int32)">
            <summary>
            Loads the supported, OData-specific serializable annotations into their in-memory representations.
            </summary>
            <param name="model">The <see cref="T:Microsoft.Data.Edm.IEdmModel"/> containing the annotations.</param>
            <param name="entityType">The <see cref="T:Microsoft.Data.Edm.IEdmEntityType"/> to process.</param>
            <param name="maxEntityPropertyMappingsPerType">The maximum number of entity mapping attributes to be found 
            for an entity type (on the type itself and all its base types).</param>
        </member>
        <member name="M:Microsoft.Data.OData.ODataUtils.SaveODataAnnotations(Microsoft.Data.Edm.IEdmModel)">
            <summary>
            Turns the in-memory representations of the supported, OData-specific annotations into their serializable form.
            </summary>
            <param name="model">The <see cref="T:Microsoft.Data.Edm.IEdmModel"/> to process.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ODataUtils.SaveODataAnnotations(Microsoft.Data.Edm.IEdmModel,Microsoft.Data.Edm.IEdmEntityType)">
            <summary>
            Turns the in-memory representations of the supported, OData-specific annotations into their serializable form.
            </summary>
            <param name="model">The <see cref="T:Microsoft.Data.Edm.IEdmModel"/> containing the annotations.</param>
            <param name="entityType">The <see cref="T:Microsoft.Data.Edm.IEdmEntityType"/> to process.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ODataUtils.HasDefaultStream(Microsoft.Data.Edm.IEdmModel,Microsoft.Data.Edm.IEdmEntityType)">
            <summary>
            Checks whether the <paramref name="entityType"/> has a default stream.
            </summary>
            <param name="model">The <see cref="T:Microsoft.Data.Edm.IEdmModel"/> containing the annotation.</param>
            <param name="entityType">The <see cref="T:Microsoft.Data.Edm.IEdmEntityType"/> to check.</param>
            <returns>true if the entity type has a default stream; otherwise false.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataUtils.SetHasDefaultStream(Microsoft.Data.Edm.IEdmModel,Microsoft.Data.Edm.IEdmEntityType,System.Boolean)">
            <summary>
            Adds or removes a default stream to/from the <paramref name="entityType"/>.
            </summary>
            <param name="model">The <see cref="T:Microsoft.Data.Edm.IEdmModel"/> containing the annotation.</param>
            <param name="entityType">The <see cref="T:Microsoft.Data.Edm.IEdmEntityType"/> to modify.</param>
            <param name="hasStream">true to add a default stream to the entity type; false to remove an existing default stream (if any).</param>
        </member>
        <member name="M:Microsoft.Data.OData.ODataUtils.IsDefaultEntityContainer(Microsoft.Data.Edm.IEdmModel,Microsoft.Data.Edm.IEdmEntityContainer)">
            <summary>
            Checks whether the <paramref name="entityContainer"/> is the default entity container.
            </summary>
            <param name="model">The <see cref="T:Microsoft.Data.Edm.IEdmModel"/> containing the annotation.</param>
            <param name="entityContainer">The <see cref="T:Microsoft.Data.Edm.IEdmEntityContainer"/> to check.</param>
            <returns>true if the <paramref name="entityContainer"/> is the default container; otherwise false.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataUtils.SetIsDefaultEntityContainer(Microsoft.Data.Edm.IEdmModel,Microsoft.Data.Edm.IEdmEntityContainer,System.Boolean)">
            <summary>
            Adds or removes a default stream to/from the <paramref name="entityContainer"/>.
            </summary>
            <param name="model">The <see cref="T:Microsoft.Data.Edm.IEdmModel"/> containing the annotation.</param>
            <param name="entityContainer">The <see cref="T:Microsoft.Data.Edm.IEdmEntityContainer"/> to modify.</param>
            <param name="isDefaultContainer">true to set the <paramref name="entityContainer"/> as the default container; false to remove an existing default container annotation (if any).</param>
        </member>
        <member name="M:Microsoft.Data.OData.ODataUtils.GetMimeType(Microsoft.Data.Edm.IEdmModel,Microsoft.Data.Edm.IEdmElement)">
            <summary>
            Checks whether the <paramref name="annotatable"/> has a MIME type annotation.
            </summary>
            <param name="model">The <see cref="T:Microsoft.Data.Edm.IEdmModel"/> containing the annotation.</param>
            <param name="annotatable">The <see cref="T:Microsoft.Data.Edm.IEdmElement"/> to check.</param>
            <returns>The (non-null) value of the MIME type annotation of the <paramref name="annotatable"/> or null if no MIME type annotation exists.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataUtils.SetMimeType(Microsoft.Data.Edm.IEdmModel,Microsoft.Data.Edm.IEdmElement,System.String)">
            <summary>
            Sets the MIME type annotation of the <paramref name="annotatable"/> to <paramref name="mimeType"/>.
            </summary>
            <param name="model">The <see cref="T:Microsoft.Data.Edm.IEdmModel"/> containing the annotation.</param>
            <param name="annotatable">The <see cref="T:Microsoft.Data.Edm.IEdmElement"/> to modify.</param>
            <param name="mimeType">The MIME type value to set as annotation value; if null, an existing annotation will be removed.</param>
            <remarks>The MIME type annotation is only supported on service operations and primitive properties for serialization purposes.</remarks>
        </member>
        <member name="M:Microsoft.Data.OData.ODataUtils.GetHttpMethod(Microsoft.Data.Edm.IEdmModel,Microsoft.Data.Edm.IEdmElement)">
            <summary>
            Checks whether the <paramref name="annotatable"/> has an HttpMethod annotation.
            </summary>
            <param name="model">The <see cref="T:Microsoft.Data.Edm.IEdmModel"/> containing the annotation.</param>
            <param name="annotatable">The <see cref="T:Microsoft.Data.Edm.IEdmElement"/> to check.</param>
            <returns>The (non-null) value of the HttpMethod annotation of the <paramref name="annotatable"/> or null if no such annotation exists.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataUtils.SetHttpMethod(Microsoft.Data.Edm.IEdmModel,Microsoft.Data.Edm.IEdmElement,System.String)">
            <summary>
            Sets the HttpMethod annotation of the <paramref name="annotatable"/> to <paramref name="httpMethod"/>.
            </summary>
            <param name="model">The <see cref="T:Microsoft.Data.Edm.IEdmModel"/> contatining the annotation.</param>
            <param name="annotatable">The <see cref="T:Microsoft.Data.Edm.IEdmElement"/> to modify.</param>
            <param name="httpMethod">The HttpMethod value to set as annotation value; if null, an existing annotation will be removed.</param>
            <remarks>The HttpMethod annotation is only supported on service operations for serialization purposes.</remarks>
        </member>
        <member name="M:Microsoft.Data.OData.ODataUtils.IsAlwaysBindable(Microsoft.Data.Edm.IEdmModel,Microsoft.Data.Edm.IEdmFunctionImport)">
            <summary>
            Gets the value of IsAlwaysBindable annotation on the <paramref name="functionImport"/>.
            </summary>
            <param name="model">The <see cref="T:Microsoft.Data.Edm.IEdmModel"/> containing the annotation.</param>
            <param name="functionImport">The <see cref="T:Microsoft.Data.Edm.IEdmFunctionImport"/> to get the annotation from.</param>
            <returns>The value of the annotation if it exists; false otherwise.</returns>
            <exception cref="T:Microsoft.Data.OData.ODataException">Thrown if the IsAlwaysBindable annotation is set to true for a non-bindable <paramref name="functionImport"/>.</exception>
        </member>
        <member name="M:Microsoft.Data.OData.ODataUtils.SetIsAlwaysBindable(Microsoft.Data.Edm.IEdmModel,Microsoft.Data.Edm.IEdmFunctionImport,System.Boolean)">
            <summary>
            Sets the value of IsAlwaysBindable annotation of the <paramref name="functionImport"/> to <paramref name="isAlwaysBindable"/>
            </summary>
            <param name="model">The <see cref="T:Microsoft.Data.Edm.IEdmModel"/> containing the annotation.</param>
            <param name="functionImport">The <see cref="T:Microsoft.Data.Edm.IEdmFunctionImport"/> to set the annotation on.</param>
            <param name="isAlwaysBindable">The value of the annotation to set.</param>
            <exception cref="T:Microsoft.Data.OData.ODataException">Thrown if IsAlwaysBindable is set to true for a non-bindable <paramref name="functionImport"/>.</exception>
        </member>
        <member name="M:Microsoft.Data.OData.ODataUtils.NullValueReadBehaviorKind(Microsoft.Data.Edm.IEdmModel,Microsoft.Data.Edm.IEdmProperty)">
            <summary>
            Gets the reader behavior for null property value on the specified property.
            </summary>
            <param name="model">The model containing the annotation.</param>
            <param name="property">The property to check.</param>
            <returns>The behavior to use when reading null value for this property.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataUtils.SetNullValueReaderBehavior(Microsoft.Data.Edm.IEdmModel,Microsoft.Data.Edm.IEdmProperty,Microsoft.Data.OData.Metadata.ODataNullValueBehaviorKind)">
            <summary>
            Adds a transient annotation to indicate how null values for the specified property should be read.
            </summary>
            <param name="model">The <see cref="T:Microsoft.Data.Edm.IEdmModel"/> containing the annotations.</param>
            <param name="property">The <see cref="T:Microsoft.Data.Edm.IEdmProperty"/> to modify.</param>
            <param name="nullValueReadBehaviorKind">The new behavior for reading null values for this property.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ODataUtils.ODataVersionToString(Microsoft.Data.OData.ODataVersion)">
            <summary>Displays the OData version to string representation.</summary>
            <returns>The OData version.</returns>
            <param name="version">The OData version.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ODataUtils.StringToODataVersion(System.String)">
            <summary>Displays a string to OData version representation.</summary>
            <returns>The OData version.</returns>
            <param name="version">The OData version.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ODataUtils.CreateAnnotationFilter(System.String)">
            <summary>
            Translates the <paramref name="annotationFilter"/> to a func that would evalutate whether the filter would match a given annotation name.
            The func would evaluate to true if the <paramref name="annotationFilter"/> matches the annotation name that's passed to the it, and false otherwise.
            </summary>
            <param name="annotationFilter">
            The filter string may be a comma delimited list of any of the following supported patterns:
              "*"        -- Matches all annotation names.
              "ns.*"     -- Matches all annotation names under the namespace "ns".
              "ns.name"  -- Matches only the annotation name "ns.name".
              "-"        -- The exclude operator may be used with any of the supported pattern, for example:
                            "-ns.*"    -- Excludes all annotation names under the namespace "ns".
                            "-ns.name" -- Excludes only the annotation name "ns.name".
            Null or empty filter is equivalent to "-*".
            
            The relative priority of the pattern is base on the relative specificity of the patterns being compared. If pattern1 is under the namespace pattern2,
            pattern1 is more specific than pattern2 because pattern1 matches a subset of what pattern2 matches. We give higher priority to the pattern that is more specific.
            For example:
             "ns.*" has higher priority than "*"
             "ns.name" has higher priority than "ns.*"
             "ns1.name" has same priority as "ns2.*"
            
            Patterns with the exclude operator takes higher precedence than the same pattern without.
            For example: "-ns.name" has higher priority than "ns.name".
            
            Examples:
              "ns1.*,ns.name"       -- Matches any annotation name under the "ns1" namespace and the "ns.name" annotation.
              "*,-ns.*,ns.name"     -- Matches any annotation name outside of the "ns" namespace and only "ns.name" under the "ns" namespace.
            </param>
            <returns>Returns a func which would evaluate to true if the <paramref name="annotationFilter"/> matches the annotation name that's passed to the it,
            and false otherwise.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ODataUtils.SaveODataAnnotationsImplementation(Microsoft.Data.Edm.IEdmModel,Microsoft.Data.Edm.IEdmEntityType)">
            <summary>
            Turns the in-memory representations of the supported, OData-specific annotations into their serializable form.
            Assumes that the entity type and the model have been validated.
            </summary>
            <param name="model">The <see cref="T:Microsoft.Data.Edm.IEdmModel"/> containing the annotations.</param>
            <param name="entityType">The <see cref="T:Microsoft.Data.Edm.IEdmEntityType"/> to process.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ODataUtils.TryGetBooleanAnnotation(Microsoft.Data.Edm.IEdmModel,Microsoft.Data.Edm.IEdmStructuredType,System.String,System.Boolean,System.Boolean@)">
            <summary>
            Gets a boolean value for the <paramref name="annotationLocalName"/> OData metadata annotation on 
            the <paramref name="structuredType"/>.
            </summary>
            <param name="model">The model containing the annotation.</param>
            <param name="structuredType">The annotatable to get the annotation from.</param>
            <param name="annotationLocalName">The local name of the annotation to get.</param>
            <param name="recursive">true to search the base type hierarchy of the structured type for the annotation; otherwise false.</param>
            <param name="boolValue">true if the annotation exists and has the value 'true'; false if the annotation does not exist or has the value 'false'.</param>
            <returns>true if the annotation with the specified local names exists; otherwise false.</returns>
            <remarks>If the annotation exists but does not have a valid boolean value this method will throw.</remarks>
        </member>
        <member name="M:Microsoft.Data.OData.ODataUtils.TryGetBooleanAnnotation(Microsoft.Data.Edm.IEdmModel,Microsoft.Data.Edm.IEdmElement,System.String,System.Boolean@)">
            <summary>
            Gets a boolean value for the <paramref name="annotationLocalName"/> OData metadata annotation on 
            the <paramref name="annotatable"/>.
            </summary>
            <param name="model">The model containing the annotation.</param>
            <param name="annotatable">The annotatable to get the annotation from.</param>
            <param name="annotationLocalName">The local name of the annotation to get.</param>
            <param name="boolValue">true if the annotation exists and has the value 'true'; false if the annotation does not exist or has the value 'false'.</param>
            <returns>true if the annotation with the specified local names exists; otherwise false.</returns>
            <remarks>If the annotation exists but does not have a valid boolean value this method wil throw.</remarks>
        </member>
        <member name="M:Microsoft.Data.OData.ODataUtils.SetBooleanAnnotation(Microsoft.Data.Edm.IEdmModel,Microsoft.Data.Edm.IEdmElement,System.String,System.Boolean)">
            <summary>
            Sets the <paramref name="boolValue "/> as value of the <paramref name="annotationLocalName"/> annotation
            on the <paramref name="annotatable"/>.
            </summary>
            <param name="model">The model containing the annotation.</param>
            <param name="annotatable">The annotatable to set the annotation on.</param>
            <param name="annotationLocalName">The local name of the annotation to set.</param>
            <param name="boolValue">The value of the annotation to set.</param>
        </member>
        <member name="T:Microsoft.Data.OData.BufferedReadStream">
            <summary>
            Class which takes an input stream, buffers the entire content asynchronously and exposes it as a stream
            which can be read synchronously.
            </summary>
        </member>
        <member name="F:Microsoft.Data.OData.BufferedReadStream.buffers">
            <summary>
            List of buffers which store the data.
            </summary>
        </member>
        <member name="F:Microsoft.Data.OData.BufferedReadStream.inputStream">
            <summary>
            The input stream to read from. This is used only during the buffering and is set to null once we've buffered everything.
            </summary>
        </member>
        <member name="F:Microsoft.Data.OData.BufferedReadStream.currentBufferIndex">
            <summary>
            Points to the buffer currently being processed.
            When writing into the buffers this points to the last buffer to which the bytes should be written.
            When reading from the buffers this points to the buffer from which we are currently reading.
            </summary>
        </member>
        <member name="F:Microsoft.Data.OData.BufferedReadStream.currentBufferReadCount">
            <summary>
            Number of bytes read from the current buffer.
            </summary>
        </member>
        <member name="M:Microsoft.Data.OData.BufferedReadStream.#ctor(System.IO.Stream)">
            <summary>
            Private constructor.
            </summary>
            <param name="inputStream">The stream to read from.</param>
        </member>
        <member name="M:Microsoft.Data.OData.BufferedReadStream.Flush">
            <summary>
            Flush the stream to the underlying storage. This operation is not supported by this stream.
            </summary>
        </member>
        <member name="M:Microsoft.Data.OData.BufferedReadStream.Read(System.Byte[],System.Int32,System.Int32)">
            <summary>
            Reads data from the stream.
            </summary>
            <param name="buffer">The buffer to read the data to.</param>
            <param name="offset">The offset in the buffer to write to.</param>
            <param name="count">The number of bytes to read.</param>
            <returns>The number of bytes actually read.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.BufferedReadStream.Seek(System.Int64,System.IO.SeekOrigin)">
            <summary>
            Seeks the stream. This operation is not supported by this stream.
            </summary>
            <param name="offset">The offset to seek to.</param>
            <param name="origin">The origin of the seek operation.</param>
            <returns>The new position in the stream.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.BufferedReadStream.SetLength(System.Int64)">
            <summary>
            Sets the length of the stream. This operation is not supported by this stream.
            </summary>
            <param name="value">The length in bytes to set.</param>
        </member>
        <member name="M:Microsoft.Data.OData.BufferedReadStream.Write(System.Byte[],System.Int32,System.Int32)">
            <summary>
            Writes to the stream. This operation is not supported by this stream.
            </summary>
            <param name="buffer">The buffer to get data from.</param>
            <param name="offset">The offset in the buffer to start from.</param>
            <param name="count">The number of bytes to write.</param>
        </member>
        <member name="M:Microsoft.Data.OData.BufferedReadStream.BufferStreamAsync(System.IO.Stream)">
            <summary>
            Given the <paramref name="inputStream"/> this method returns a task which will asynchronously
            read the entire content of that stream and return a new synchronous stream from which the data can be read.
            </summary>
            <param name="inputStream">The input stream to asynchronously buffer.</param>
            <returns>A task which returns the buffered stream.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.BufferedReadStream.ResetForReading">
            <summary>
            Resets the stream to the begining and prepares it for reading.
            </summary>
        </member>
        <member name="M:Microsoft.Data.OData.BufferedReadStream.Dispose(System.Boolean)">
            <summary>
            Disposes the object.
            </summary>
            <param name="disposing">True if called from Dispose; false if called from the finalizer.</param>
        </member>
        <member name="M:Microsoft.Data.OData.BufferedReadStream.BufferInputStream">
            <summary>
            Returns enumeration of tasks to run to buffer the entire input stream.
            </summary>
            <returns>Enumeration of tasks to run to buffer the input stream.</returns>
            <remarks>This method relies on lazy eval of the enumerator, never enumerate through it synchronously.</remarks>
        </member>
        <member name="M:Microsoft.Data.OData.BufferedReadStream.AddNewBuffer">
            <summary>
            Adds a new buffer to the list and makes it the current buffer.
            </summary>
            <returns>The newly added buffer.</returns>
        </member>
        <member name="P:Microsoft.Data.OData.BufferedReadStream.CanRead">
            <summary>
            Determines if the stream can read - this one can
            </summary>
        </member>
        <member name="P:Microsoft.Data.OData.BufferedReadStream.CanSeek">
            <summary>
            Determines if the stream can seek - this one cannot
            </summary>
        </member>
        <member name="P:Microsoft.Data.OData.BufferedReadStream.CanWrite">
            <summary>
            Determines if the stream can write - this one cannot
            </summary>
        </member>
        <member name="P:Microsoft.Data.OData.BufferedReadStream.Length">
            <summary>
            Returns the length of the stream, which this implementation doesn't support.
            </summary>
        </member>
        <member name="P:Microsoft.Data.OData.BufferedReadStream.Position">
            <summary>
            Gets or sets the position in the stream, this stream doesn't support seeking, so position is also unsupported.
            </summary>
        </member>
        <member name="T:Microsoft.Data.OData.BufferedReadStream.DataBuffer">
            <summary>
            Class to wrap a byte buffer used to store portion of the buffered data.
            </summary>
        </member>
        <member name="F:Microsoft.Data.OData.BufferedReadStream.DataBuffer.MinReadBufferSize">
            <summary>
            The minimum size to ask for when reading from underlying stream.
            </summary>
        </member>
        <member name="F:Microsoft.Data.OData.BufferedReadStream.DataBuffer.BufferSize">
            <summary>
            The size of a buffer to allocate - use 64KB to be aligned which makes it likely that the underlying levels
            will be able to process the request in one go.
            </summary>
        </member>
        <member name="F:Microsoft.Data.OData.BufferedReadStream.DataBuffer.buffer">
            <summary>
            The byte buffer which stored the data.
            </summary>
        </member>
        <member name="M:Microsoft.Data.OData.BufferedReadStream.DataBuffer.#ctor">
            <summary>
            Constructor - creates a new buffer;
            </summary>
        </member>
        <member name="M:Microsoft.Data.OData.BufferedReadStream.DataBuffer.MarkBytesAsWritten(System.Int32)">
            <summary>
            Marks specified count of bytes as written starting at the OffsetToWriteTo.
            </summary>
            <param name="count">The number of bytes to mark as written.</param>
        </member>
        <member name="P:Microsoft.Data.OData.BufferedReadStream.DataBuffer.Buffer">
            <summary>
            The byte buffer.
            </summary>
        </member>
        <member name="P:Microsoft.Data.OData.BufferedReadStream.DataBuffer.OffsetToWriteTo">
            <summary>
            The offset into the buffer to which more data can be written.
            </summary>
        </member>
        <member name="P:Microsoft.Data.OData.BufferedReadStream.DataBuffer.StoredCount">
            <summary>
            The number of bytes stored in the buffer.
            </summary>
        </member>
        <member name="P:Microsoft.Data.OData.BufferedReadStream.DataBuffer.FreeBytes">
            <summary>
            The number of bytes not yet used in the buffer.
            </summary>
        </member>
        <member name="T:Microsoft.Data.OData.Utils">
            <summary>
            Generic  utility methods.
            </summary>
        </member>
        <member name="M:Microsoft.Data.OData.Utils.TryDispose(System.Object)">
            <summary>
            Calls IDisposable.Dispose() on the argument if it is not null 
            and is an IDisposable.
            </summary>
            <param name="o">The instance to dispose.</param>
            <returns>'True' if IDisposable.Dispose() was called; 'false' otherwise.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.Utils.FlushAsync(System.IO.Stream)">
            <summary>
            Asynchronously flushes a stream.
            </summary>
            <param name="stream">The stream to flush.</param>
            <returns>Task which represents the pending Flush operation.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.Utils.StableSort``1(``0[],System.Comparison{``0})">
            <summary>
            Perform a stable sort of the <paramref name="array"/> using the specified <paramref name="comparison"/>.
            </summary>
            <typeparam name="T">The type of the items in the array to sort.</typeparam>
            <param name="array">The array to sort.</param>
            <param name="comparison">The comparison to use to compare items in the array</param>
            <returns>Array of KeyValuePairs where the sequence of Values is the sorted representation of <paramref name="array"/>.</returns>
        </member>
        <member name="T:Microsoft.Data.OData.Utils.StableComparer`1">
            <summary>
            Stable comparer of a sequence of key/value pairs where each pair 
            knows its position in the sequence and its value.
            </summary>
            <typeparam name="T">The type of the values in the sequence.</typeparam>
        </member>
        <member name="F:Microsoft.Data.OData.Utils.StableComparer`1.innerComparer">
            <summary>
            The <see cref="T:System.Comparison`1"/> to compare the values.
            </summary>
        </member>
        <member name="M:Microsoft.Data.OData.Utils.StableComparer`1.#ctor(System.Comparison{`0})">
            <summary>
            Constructor.
            </summary>
            <param name="innerComparer">The <see cref="T:System.Comparison`1"/> to compare the values.</param>
        </member>
        <member name="M:Microsoft.Data.OData.Utils.StableComparer`1.Compare(System.Collections.Generic.KeyValuePair{System.Int32,`0},System.Collections.Generic.KeyValuePair{System.Int32,`0})">
            <summary>
            Compares two key/value pairs by first comparing their value. If the values are equal,
            the position in the array determines the relative order (and preserves the original relative order).
            </summary>
            <param name="x">First key/value pair.</param>
            <param name="y">Second key/value pair.</param>
            <returns>
            A value &lt; 0 if <paramref name="x"/> is less than <paramref name="y"/>.
            The value 0 if <paramref name="x"/> is equal to <paramref name="y"/>. Note this only happens when comparing the same items when used in StableSort.
            A value &gt; 0 if <paramref name="x"/> is greater than <paramref name="y"/>.
            </returns>
            <remarks>This method will never return the value 0 since the input sequence is constructed in a way
            that all key/value pairs have unique indeces.</remarks>
        </member>
        <member name="T:Microsoft.Data.OData.Metadata.MetadataUtils">
            <summary>
            Class with utility methods for dealing with OData metadata.
            </summary>
        </member>
        <member name="M:Microsoft.Data.OData.Metadata.MetadataUtils.TryGetODataAnnotation(Microsoft.Data.Edm.IEdmModel,Microsoft.Data.Edm.IEdmElement,System.String,System.String@)">
            <summary>
            Returns the annotation in the OData metadata namespace with the specified <paramref name="localName"/>.
            </summary>
            <param name="model">The <see cref="T:Microsoft.Data.Edm.IEdmModel"/> containing the annotation.</param>
            <param name="annotatable">The <see cref="T:Microsoft.Data.Edm.IEdmElement"/> to get the annotation from.</param>
            <param name="localName">The local name of the annotation to find.</param>
            <param name="value">The value of the annotation in the OData metadata namespace and with the specified <paramref name="localName"/>.</param>
            <returns>true if an annotation with the specified local name was found; otherwise false.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.Metadata.MetadataUtils.SetODataAnnotation(Microsoft.Data.Edm.IEdmModel,Microsoft.Data.Edm.IEdmElement,System.String,System.String)">
            <summary>
            Sets the annotation with the OData metadata namespace and the specified <paramref name="localName"/> on the <paramref name="annotatable"/>.
            </summary>
            <param name="model">The <see cref="T:Microsoft.Data.Edm.IEdmModel"/> containing the annotations."/&gt;</param>
            <param name="annotatable">The <see cref="T:Microsoft.Data.Edm.IEdmElement"/> to set the annotation on.</param>
            <param name="localName">The local name of the annotation to set.</param>
            <param name="value">The value of the annotation to set.</param>
        </member>
        <member name="M:Microsoft.Data.OData.Metadata.MetadataUtils.GetODataAnnotations(Microsoft.Data.Edm.IEdmModel,Microsoft.Data.Edm.IEdmElement)">
            <summary>
            Gets all the serializable annotations in the OData metadata namespace on the <paramref name="annotatable"/>.
            </summary>
            <param name="model">The <see cref="T:Microsoft.Data.Edm.IEdmModel"/> containing the annotations."/&gt;</param>
            <param name="annotatable">The <see cref="T:Microsoft.Data.Edm.IEdmElement"/> to get the annotations from.</param>
            <returns>All annotations in the OData metadata namespace; or null if no annotations are found.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.Metadata.MetadataUtils.GetEdmType(Microsoft.Data.OData.ODataAnnotatable)">
            <summary>
            Gets the EDM type of an OData instance from the <see cref="T:Microsoft.Data.OData.ODataTypeAnnotation"/> of the instance (if available).
            </summary>
            <param name="annotatable">The OData instance to get the EDM type for.</param>
            <returns>The EDM type of the <paramref name="annotatable"/> if available in the <see cref="T:Microsoft.Data.OData.ODataTypeAnnotation"/> annotation.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.Metadata.MetadataUtils.ResolveTypeNameForWrite(Microsoft.Data.Edm.IEdmModel,System.String)">
            <summary>
            Resolves the name of a primitive, complex, entity or collection type to the respective type. Uses the semantics used by writers.
            Thus it implements the strict speced behavior.
            </summary>
            <param name="model">The model to use.</param>
            <param name="typeName">The name of the type to resolve.</param>
            <returns>The <see cref="T:Microsoft.Data.Edm.IEdmType"/> representing the type specified by the <paramref name="typeName"/>;
            or null if no such type could be found.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.Metadata.MetadataUtils.ResolveTypeNameForRead(Microsoft.Data.Edm.IEdmModel,Microsoft.Data.Edm.IEdmType,System.String,Microsoft.Data.OData.ODataReaderBehavior,Microsoft.Data.OData.ODataVersion,Microsoft.Data.Edm.EdmTypeKind@)">
            <summary>
            Resolves the name of a primitive, complex, entity or collection type to the respective type. Uses the semantics used be readers.
            Thus it can be a bit looser.
            </summary>
            <param name="model">The model to use.</param>
            <param name="expectedType">The expected type for the type name being resolved, or null if none is available.</param>
            <param name="typeName">The name of the type to resolve.</param>
            <param name="readerBehavior">Reader behavior if the caller is a reader, null if no reader behavior is available.</param>
            <param name="version">The version of the payload being read.</param>
            <param name="typeKind">The type kind of the type, if it could be determined. This will be None if we couldn't tell. It might be filled
            even if the method returns null, for example for Collection types with item types which are not recognized.</param>
            <returns>The <see cref="T:Microsoft.Data.Edm.IEdmType"/> representing the type specified by the <paramref name="typeName"/>;
            or null if no such type could be found.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.Metadata.MetadataUtils.ResolveTypeName(Microsoft.Data.Edm.IEdmModel,Microsoft.Data.Edm.IEdmType,System.String,System.Func{Microsoft.Data.Edm.IEdmType,System.String,Microsoft.Data.Edm.IEdmType},Microsoft.Data.OData.ODataVersion,Microsoft.Data.Edm.EdmTypeKind@)">
            <summary>
            Resolves the name of a primitive, complex, entity or collection type to the respective type.
            </summary>
            <param name="model">The model to use.</param>
            <param name="expectedType">The expected type for the type name being resolved, or null if none is available.</param>
            <param name="typeName">The name of the type to resolve.</param>
            <param name="customTypeResolver">Custom type resolver to use, if null the model is used directly.</param>
            <param name="version">The version to use when resolving the type name.</param>
            <param name="typeKind">The type kind of the type, if it could be determined. This will be None if we couldn't tell. It might be filled
            even if the method returns null, for example for Collection types with item types which are not recognized.</param>
            <returns>The <see cref="T:Microsoft.Data.Edm.IEdmType"/> representing the type specified by the <paramref name="typeName"/>;
            or null if no such type could be found.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.Metadata.MetadataUtils.CalculateAlwaysBindableOperationsForType(Microsoft.Data.Edm.IEdmType,Microsoft.Data.Edm.IEdmModel,Microsoft.Data.OData.Metadata.EdmTypeResolver)">
            <summary>
            Calculates the operations that are always bindable to the given type.
            </summary>
            <param name="bindingType">The binding type in question.</param>
            <param name="model">The model to search for operations.</param>
            <param name="edmTypeResolver">The edm type resolver to get the parameter type.</param>
            <returns>An enumeration of operations that are always bindable to the given type.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.Metadata.MetadataUtils.LookupTypeOfValueTerm(System.String,Microsoft.Data.Edm.IEdmModel)">
            <summary>
            Looks up the given term name in the given model, and returns the term's type if a matching term was found.
            </summary>
            <param name="qualifiedTermName">The name of the term to lookup, including the namespace.</param>
            <param name="model">The model to look in.</param>
            <returns>The type of the term in the model, or null if no matching term was found.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.Metadata.MetadataUtils.GetNullablePayloadTypeReference(Microsoft.Data.Edm.IEdmType)">
            <summary>
            Gets the nullable type reference for a payload type; if the payload type is null, uses Edm.String.
            </summary>
            <param name="payloadType">The payload type to get the type reference for.</param>
            <returns>The nullable <see cref="T:Microsoft.Data.Edm.IEdmTypeReference"/> for the <paramref name="payloadType"/>.</returns>
        </member>
        <member name="T:Microsoft.Data.OData.ValidationUtils">
            <summary>
            Class with utility methods for validating OData content (applicable for readers and writers).
            </summary>
        </member>
        <member name="F:Microsoft.Data.OData.ValidationUtils.MaxBoundaryLength">
            <summary>Maximum batch boundary length supported (not includeding leading CRLF or '-').</summary>
        </member>
        <member name="F:Microsoft.Data.OData.ValidationUtils.InvalidCharactersInPropertyNames">
            <summary>The set of characters that are invalid in property names.</summary>
            <remarks>Keep this array in sync with MetadataProviderUtils.InvalidCharactersInPropertyNames in Astoria.</remarks>
        </member>
        <member name="M:Microsoft.Data.OData.ValidationUtils.ValidateOpenPropertyValue(System.String,System.Object)">
            <summary>
            Validates that an open property value is supported.
            </summary>
            <param name="propertyName">The name of the open property.</param>
            <param name="value">The value of the open property.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ValidationUtils.ValidateValueTypeKind(Microsoft.Data.Edm.EdmTypeKind,System.String)">
            <summary>
            Validates a type kind for a value type.
            </summary>
            <param name="typeKind">The type kind.</param>
            <param name="typeName">The name of the type (used for error reporting only).</param>
        </member>
        <member name="M:Microsoft.Data.OData.ValidationUtils.ValidateCollectionTypeName(System.String)">
            <summary>
            Validates that <paramref name="collectionTypeName"/> is a valid type name for a collection and returns its item type name.
            </summary>
            <param name="collectionTypeName">The name of the collection type.</param>
            <returns>The item type name for the <paramref name="collectionTypeName"/>.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ValidationUtils.ValidateEntityTypeIsAssignable(Microsoft.Data.Edm.IEdmEntityTypeReference,Microsoft.Data.Edm.IEdmEntityTypeReference)">
            <summary>
            Validates that the <paramref name="payloadEntityTypeReference"/> is assignable to the <paramref name="expectedEntityTypeReference"/>
            and fails if it's not.
            </summary>
            <param name="expectedEntityTypeReference">The expected entity type reference, the base type of the entities expected.</param>
            <param name="payloadEntityTypeReference">The payload entity type reference to validate.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ValidationUtils.ValidateCollectionType(Microsoft.Data.Edm.IEdmTypeReference)">
            <summary>
            Validates that the <paramref name="typeReference"/> represents a collection type.
            </summary>
            <param name="typeReference">The type reference to validate.</param>
            <returns>The <see cref="T:Microsoft.Data.Edm.IEdmCollectionTypeReference"/> instance representing the collection passed as <paramref name="typeReference"/>.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ValidationUtils.ValidateCollectionItem(System.Object,System.Boolean)">
            <summary>
            Validates an item of a collection to ensure it is not of collection and stream reference types.
            </summary>
            <param name="item">The collection item.</param>
            <param name="isStreamable">True if the items in the collection are streamable, false otherwise.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ValidationUtils.ValidateNullCollectionItem(Microsoft.Data.Edm.IEdmTypeReference,Microsoft.Data.OData.ODataWriterBehavior)">
            <summary>
            Validates a null collection item against the expected type.
            </summary>
            <param name="expectedItemType">The expected item type or null if no expected item type exists.</param>
            <param name="writerBehavior">The <see cref="T:Microsoft.Data.OData.ODataWriterBehavior"/> instance controlling the behavior of the writer.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ValidationUtils.ValidateStreamReferenceProperty(Microsoft.Data.OData.ODataProperty,Microsoft.Data.Edm.IEdmProperty)">
            <summary>
            Validates a stream reference property to ensure it's not null and its name if correct.
            </summary>
            <param name="streamProperty">The stream reference property to validate.</param>
            <param name="edmProperty">Property metadata to validate against.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ValidationUtils.ValidateAssociationLinkNotNull(Microsoft.Data.OData.ODataAssociationLink)">
            <summary>
            Validates an <see cref="T:Microsoft.Data.OData.ODataAssociationLink"/> to ensure it's not null.
            </summary>
            <param name="associationLink">The association link to ensure it's not null.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ValidationUtils.ValidateAssociationLinkName(System.String)">
            <summary>
            Validates the name for an association link.
            </summary>
            <param name="associationLinkName">The name of the association link to validate.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ValidationUtils.ValidateAssociationLink(Microsoft.Data.OData.ODataAssociationLink)">
            <summary>
            Validates an <see cref="T:Microsoft.Data.OData.ODataAssociationLink"/> to ensure all required information is specified and valid.
            </summary>
            <param name="associationLink">The association link to validate.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ValidationUtils.IncreaseAndValidateRecursionDepth(System.Int32@,System.Int32)">
            <summary>
            Increases the given recursion depth, and then verifies that it doesn't exceed the recursion depth limit.
            </summary>
            <param name="recursionDepth">The current depth of the payload element hierarchy.</param>
            <param name="maxDepth">The maximum allowed recursion depth.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ValidationUtils.ValidateOperationNotNull(Microsoft.Data.OData.ODataOperation,System.Boolean)">
            <summary>
            Validates an <see cref="T:Microsoft.Data.OData.ODataOperation"/> to ensure it's not null.
            </summary>
            <param name="operation">The operation to ensure it's not null.</param>
            <param name="isAction">Whether <paramref name="operation"/> is an <see cref="T:Microsoft.Data.OData.ODataAction"/>.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ValidationUtils.ValidateOperationMetadataNotNull(Microsoft.Data.OData.ODataOperation)">
            <summary>
            Validates an <see cref="T:Microsoft.Data.OData.ODataOperation"/> to ensure its metadata is specified and valid.
            </summary>
            <param name="operation">The operation to validate.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ValidationUtils.ValidateOperationTargetNotNull(Microsoft.Data.OData.ODataOperation)">
            <summary>
            Validates an <see cref="T:Microsoft.Data.OData.ODataOperation"/> to ensure its target is specified and valid.
            </summary>
            <param name="operation">The operation to validate.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ValidationUtils.ValidateEntryMetadataResource(Microsoft.Data.OData.ODataEntry,Microsoft.Data.Edm.IEdmEntityType,Microsoft.Data.Edm.IEdmModel,System.Boolean)">
            <summary>
            Validates that the specified <paramref name="entry"/> is a valid entry as per the specified type.
            </summary>
            <param name="entry">The entry to validate.</param>
            <param name="entityType">Optional entity type to validate the entry against.</param>
            <param name="model">Model containing the entity type.</param>
            <param name="validateMediaResource">true if the validation of the default MediaResource should be done; false otherwise.</param>
            <remarks>If the <paramref name="entityType"/> is available only entry-level tests are performed, properties and such are not validated.</remarks>
        </member>
        <member name="M:Microsoft.Data.OData.ValidationUtils.ValidateIsExpectedPrimitiveType(System.Object,Microsoft.Data.Edm.IEdmTypeReference)">
            <summary>
            Validates that a given primitive value is of the expected (primitive) type.
            </summary>
            <param name="value">The value to check.</param>
            <param name="expectedTypeReference">The expected type for the value.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ValidationUtils.ValidateIsExpectedPrimitiveType(System.Object,Microsoft.Data.Edm.IEdmPrimitiveTypeReference,Microsoft.Data.Edm.IEdmTypeReference)">
            <summary>
            Validates that a given primitive value is of the expected (primitive) type.
            </summary>
            <param name="value">The value to check.</param>
            <param name="valuePrimitiveTypeReference">The primitive type reference for the value - some callers have this already, so we save the lookup here.</param>
            <param name="expectedTypeReference">The expected type for the value.</param>
            <remarks>
            Some callers have the primitive type reference already resolved (from the value type)
            so this method is an optimized version to not lookup the primitive type reference again.
            </remarks>
        </member>
        <member name="M:Microsoft.Data.OData.ValidationUtils.ValidateMetadataPrimitiveType(Microsoft.Data.Edm.IEdmTypeReference,Microsoft.Data.Edm.IEdmTypeReference)">
            <summary>
            Validates that the expected primitive type matches the actual primitive type.
            </summary>
            <param name="expectedTypeReference">The expected type.</param>
            <param name="typeReferenceFromValue">The actual type.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ValidationUtils.ValidateResourceCollectionInfo(Microsoft.Data.OData.ODataResourceCollectionInfo)">
            <summary>
            Validates a resource collection.
            </summary>
            <param name="collectionInfo">The resource collection to validate.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ValidationUtils.ValidateResourceCollectionInfoUrl(System.String)">
            <summary>
            Validates a resource collection Url.
            </summary>
            <param name="collectionInfoUrl">The resource collection url to validate.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ValidationUtils.ValidateTypeKind(Microsoft.Data.Edm.EdmTypeKind,Microsoft.Data.Edm.EdmTypeKind,System.String)">
            <summary>
            Validates that the observed type kind is the expected type kind.
            </summary>
            <param name="actualTypeKind">The actual type kind to compare.</param>
            <param name="expectedTypeKind">The expected type kind to compare against.</param>
            <param name="typeName">The name of the type to use in the error.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ValidationUtils.ValidateBoundaryString(System.String)">
            <summary>
            Validates that a boundary delimiter is valid (non-null, less than 70 chars, only valid chars, etc.)
            </summary>
            <param name="boundary">The boundary delimiter to test.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ValidationUtils.ShouldValidateComplexPropertyNullValue(Microsoft.Data.Edm.IEdmModel)">
            <summary>
            Null validation of complex properties will be skipped if edm version is less than v3 and data service version exists.
            In such cases, the provider decides what should be done if a null value is stored on a non-nullable complex property.
            </summary>
            <param name="model">The model containing the complex property.</param>
            <returns>True if complex property should be validated for null values.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ValidationUtils.IsValidPropertyName(System.String)">
            <summary>
            Validates that a property name is valid in OData.
            </summary>
            <param name="propertyName">The property name to validate.</param>
            <returns>true if the property name is valid, otherwise false.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.ValidationUtils.ValidatePropertyName(System.String)">
            <summary>
            Validates a property name to check whether it contains reserved characters.
            </summary>
            <param name="propertyName">The property name to check.</param>
        </member>
        <member name="M:Microsoft.Data.OData.ValidationUtils.ValidateTotalEntityPropertyMappingCount(Microsoft.Data.OData.Metadata.ODataEntityPropertyMappingCache,Microsoft.Data.OData.Metadata.ODataEntityPropertyMappingCollection,System.Int32)">
            <summary>
            Validates that the total number of entity property mapping attributes on the base entity type and the current
            entity type does not exceed the specified security limit.
            </summary>
            <param name="baseCache">The EPM cache of the base entity type or null if no base entity type exists.</param>
            <param name="mappings">The EPM collection of the current entity type.</param>
            <param name="maxMappingCount">The maximum number of mappings allowed for an entity type (on the type itself and all its base types).</param>
            <returns>The total number of entity property mappings for the current entity type.</returns>
        </member>
        <member name="T:Microsoft.Data.OData.WriterUtils">
            <summary>
            Class with utility methods for writing OData content.
            </summary>
        </member>
        <member name="M:Microsoft.Data.OData.WriterUtils.ShouldSkipProperty(Microsoft.Data.OData.ProjectedPropertiesAnnotation,System.String)">
            <summary>
            Determines if a property should be written or skipped.
            </summary>
            <param name="projectedProperties">The projected properties annotation to use (can be null).</param>
            <param name="propertyName">The name of the property to check.</param>
            <returns>true if the property should be skipped, false to write the property.</returns>
        </member>
        <member name="T:Microsoft.Data.OData.MediaTypeUtils">
            <summary>
            Class with utility methods to work with media types.
            </summary>
        </member>
        <member name="F:Microsoft.Data.OData.MediaTypeUtils.allSupportedPayloadKinds">
            <summary>An array of all the supported payload kinds.</summary>
        </member>
        <member name="F:Microsoft.Data.OData.MediaTypeUtils.encodingUtf8NoPreamble">
            <summary>UTF-8 encoding, without the BOM preamble.</summary>
            <remarks>
            While a BOM preamble on UTF8 is generally benign, it seems that some MIME handlers under IE6 will not 
            process the payload correctly when included.
            
            Because the data service should include the encoding as part of the Content-Type in the response,
            there should be no ambiguity as to what encoding is being used.
            
            For further information, see http://www.unicode.org/faq/utf_bom.html#BOM.
            </remarks>
        </member>
        <member name="M:Microsoft.Data.OData.MediaTypeUtils.GetContentTypeFromSettings(Microsoft.Data.OData.ODataMessageWriterSettings,Microsoft.Data.OData.ODataPayloadKind,Microsoft.Data.OData.MediaTypeResolver,Microsoft.Data.OData.MediaType@,System.Text.Encoding@)">
            <summary>
            Given the Accept and the Accept-Charset headers of the request message computes the media type, encoding and <see cref="T:Microsoft.Data.OData.ODataFormat"/>
            to be used for the response message.
            </summary>
            <param name="settings">The message writer settings to use for serializing the response payload.</param>
            <param name="payloadKind">The kind of payload to be serialized as part of the response message.</param>
            <param name="mediaTypeResolver">The media type resolver to use when interpreting the content type.</param>
            <param name="mediaType">The media type to be used in the response message.</param>
            <param name="encoding">The encoding to be used in the response message.</param>
            <returns>The <see cref="T:Microsoft.Data.OData.ODataFormat"/> used when serializing the response.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.MediaTypeUtils.GetFormatFromContentType(System.String,Microsoft.Data.OData.ODataPayloadKind[],Microsoft.Data.OData.MediaTypeResolver,Microsoft.Data.OData.MediaType@,System.Text.Encoding@,Microsoft.Data.OData.ODataPayloadKind@,System.String@)">
            <summary>
            Determine the <see cref="T:Microsoft.Data.OData.ODataFormat"/> to use for the given <paramref name="contentTypeHeader"/>. If no supported content type
            is found an exception is thrown.
            </summary>
            <param name="contentTypeHeader">The name of the content type to be checked.</param>
            <param name="supportedPayloadKinds">All possiblel kinds of payload that can be read with this content type.</param>
            <param name="mediaTypeResolver">The media type resolver to use when interpreting the content type.</param>
            <param name="mediaType">The media type parsed from the <paramref name="contentTypeHeader"/>.</param>
            <param name="encoding">The encoding from the content type or the default encoding for the <paramref name="mediaType"/>.</param>
            <param name="selectedPayloadKind">
            The payload kind that was selected form the list of <paramref name="supportedPayloadKinds"/> for the 
            specified <paramref name="contentTypeHeader"/>.
            </param>
            <param name="batchBoundary">The batch boundary read from the content type for batch payloads; otherwise null.</param>
            <returns>The <see cref="T:Microsoft.Data.OData.ODataFormat"/> for the <paramref name="contentTypeHeader"/>.</returns>
        </member>
        <member name="M:Microsoft.Data.OData.MediaTypeUtils.GetPayloadKindsForContentType(System.String,Microsoft.Data.OData.MediaTypeResolver,Microsoft.Data.OData.MediaType@,System.Text.Encoding@)">
            <summary>
            Gets all payload kinds and their corresponding formats that match the specified content type header.
            </summary>
            <param name="contentTypeHeader">The content type header to get the payload kinds for.</param>
            <param name="mediaTypeResolver">The media type resolver to use when interpreting the content type.</param>
            <param name="contentType">The parsed content type as <see cref="T:Microsoft.Data.OData.MediaType"/>.</param>
            <param name="encoding">The encoding from the content type or the default encoding from <see cref="T:Microsoft.Data.OData.MediaType"/>.</param>
            <returns>The list of payload kinds and formats supported for the specified <paramref name="contentTypeHeader"/>.</returns>
        </member>
        <member na