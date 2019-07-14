// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

function updateMetadataOnInsert() {

    var collection = getContext().getCollection();
    var doc = getContext().getRequest().getBody();

    if (!doc.isMetadata) {
        getAndUpdateMetadata();
    }

    function getAndUpdateMetadata() {
        var docId = collection.getAltLink() + '/docs/metadata';

        var isAccepted = collection.readDocument(docId, function (err, doc) {
            if (err) throw err;

            doc.count += 1;

            // Update/replace the metadata document in the store.
            var isAccepted = collection.replaceDocument(doc._self, doc, function (err) {
                if (err) throw err;
                // Note: in case concurrent updates causes conflict with ErrorCode.RETRY_WITH, we can't read the meta again 
                //       and update again because due to Snapshot isolation we will read same exact version (we are in same transaction).
                //       We have to take care of that on the client side.
            });
            if (!isAccepted) throw new Error("The call replaceDocument(metaDoc) returned false.");
        });

        if (!isAccepted) throw new Error("The call queryDocuments for metaDoc returned false.");
    }
}