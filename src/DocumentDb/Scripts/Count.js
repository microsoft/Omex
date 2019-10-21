﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

function count(filterQuery, continuationToken) {
    var collection = getContext().getCollection();
    var maxResult = 500000;
    var result = 0;

    if (!filterQuery) {
        filterQuery = "SELECT VALUE COUNT(1) FROM root";
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
            throw new Error("Error while reading document: " + err);
        }

        var doc1 = docFeed[0];
        result += doc1;

        if (responseOptions.continuation) {
            tryQuery(responseOptions.continuation);
        } else {
            setBody(null);
        }
    }

    function setBody(nextContinuationToken) {
        var body = { count: result, continuationToken: nextContinuationToken };
        getContext().getResponse().setBody(body);
    }
}
