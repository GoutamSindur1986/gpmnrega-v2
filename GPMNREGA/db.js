// database code;
var db;
var request;


window.indexedDB = window.indexedDB || window.mozIndexedDB ||
    window.webkitIndexedDB || window.msIndexedDB;

//prefixes of window.IDB objects
window.IDBTransaction = window.IDBTransaction ||
    window.webkitIDBTransaction || window.msIDBTransaction;
window.IDBKeyRange = window.IDBKeyRange || window.webkitIDBKeyRange ||
    window.msIDBKeyRange
request = window.indexedDB.open("DPRFTable",1);
if (!window.indexedDB) {
    window.alert("Your browser doesn't support a stable version of IndexedDB.")
}

request.onerror = function (event) {
    console.log("error: ");
};

request.onsuccess = function (event) {
    db = request.result;
       console.log("success: " + db);
};

request.onupgradeneeded = function (event) {
    var db = event.target.result;

    var dprStore = db.createObjectStore("DPRFrozenStatus", { keyPath: "gpcode" });
    dprStore.onsuccess = function (e) {
        console.log("datastore created");

    }
    dprStore.onerror = function (err) {
        console.log(err + "error creating datastore;");
    }

}

function addDPRFReport(gpcode, data) {
    var request = db.transaction(["DPRFrozenStatus"], "readwrite")
        .objectStore("DPRFrozenStatus")
        .put({ gpcode: gpcode.toLowerCase(), dprtable: data });

    request.onsuccess = function (event) {
        console.log("Report data added.");
        localStorage["DPRLoad"] = "success";
        readDPRStatus(event.currentTarget.result);
    };

    request.onerror = function (event) {
        console.log("error adding report data.");
        localStorage["DPRLoad"] = undefined;
    }
}


function readDPRStatus(gpcode) {
    var transaction = db.transaction(["DPRFrozenStatus"]);
    var objectStore = transaction.objectStore("DPRFrozenStatus");
    var request = objectStore.get(gpcode.toLowerCase());

    request.onerror = function (event) {
       console.log("error connecting NREGA.")
    };

    request.onsuccess = function (event) {
        // Do something with the request.result!
        if (request.result) {
            loadDPRWorkLinks(request.result.dprtable);
            
        }
        else {
            console.log("no record in database: searching main server now...!");
           
        }
    };
}



