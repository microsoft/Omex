// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

function count(filterQuery, continuationToken) {
    var collection = getContext().getCollection();
    var maxResult = 500000; 
    var result = 0;

    var q = "SELECT VALUE COUNT(1) FROM root";
    if (!filterQuery) {
        filterQuery = q;
    }

    tryQuery(continuationToken);

    function tryQuery(nextContinuationToken) {
        var responseOptions = { continuation: nextContinuationToken, pageSize: maxResult };

        if (result >= maxResult || !query(responseOptions)) {
            setBody(nextContinuationToken);
        }
    }

    function query(responseOptions) {
        return (filterQuery && filterQuery.length) ?
            collection.queryDocuments(collection.getSelfLink(), filterQuery, responseOptions, onReadDocuments) :
            collection.readDocuments(collection.getSelfLink(), responseOptions, onReadDocuments);
    }

    function onReadDocuments(err, docFeed, responseOptions) {
        if (err) {
            throw 'Error while reading document: ' + err;
        }
        
        var doc1 = docFeed[0];
        result += doc1;

        if (responseOptions.continuation) {
            tryQuery(responseOptions.continuation);
        } else {
            setBody(null);
        }
    }

    function setBody(continuationToken) {
        var body = { count: result, continuationToken: continuationToken };
        getContext().getResponse().setBody(body);
    }
}