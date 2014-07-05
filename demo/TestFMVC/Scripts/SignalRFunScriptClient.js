var SignalRClient__start$, SignalRClient__signalR, SignalRClient__serverHub, SignalRClient__proxy, SignalRClient__printResult$, SignalRClient__main$, SignalRClient__log, SignalRClient__j$, SignalRClient__get_signalR$, SignalRClient__get_serverHub$, SignalRClient__get_proxy$, SignalRClient__get_log$, LanguagePrimitives__UnboxGeneric$String_String, LanguagePrimitives__UnboxGeneric$List_1_Int32_List_1_Int32_, LanguagePrimitives__UnboxGeneric$HubConnection_HubConnection_;
  LanguagePrimitives__UnboxGeneric$HubConnection_HubConnection_ = (function (x)
  {
    return x;;
  });
  LanguagePrimitives__UnboxGeneric$List_1_Int32_List_1_Int32_ = (function (x)
  {
    return x;;
  });
  LanguagePrimitives__UnboxGeneric$String_String = (function (x)
  {
    return x;;
  });
  SignalRClient__get_log$ = (function ()
  {
    var objectArg = (window.console);
    return (function (arg00)
    {
      return (objectArg.log(arg00));
    });
  });
  SignalRClient__get_proxy$ = (function ()
  {
    return ((SignalRClient__signalR.hub).createHubProxy("myHub"));
  });
  SignalRClient__get_serverHub$ = (function ()
  {
    var conn = (SignalRClient__signalR.hub);
    return conn;
  });
  SignalRClient__get_signalR$ = (function ()
  {
    return ((window.$).signalR);
  });
  SignalRClient__j$ = (function (s)
  {
    return ((window.$)(s));
  });
  SignalRClient__main$ = (function (unitVar0)
  {
    ((window.console).log("##Starting:## "));
    ((SignalRClient__signalR.hub).url) = "/signalrHub";
    null;
    (function (value)
    {
      var ignored0 = value;
    })((SignalRClient__proxy.on("myCustomClientFunction", (function (args)
    {
      var s = LanguagePrimitives__UnboxGeneric$String_String(args);
      SignalRClient__printResult$(s);
      return SignalRClient__log(("Response: " + s));
    }))));
    return ((SignalRClient__signalR.hub).start((function ()
    {
      return SignalRClient__start$();
    })));
  });
  SignalRClient__printResult$ = (function (value)
  {
    var _23;
    var _24;
    var objectArg = SignalRClient__j$("#results");
    _24 = (function (arg00)
    {
      return (objectArg.append(arg00));
    });
    _23 = _24(((("\u003cp\u003e" + value) + "") + "\u003c/p\u003e"));
    return (function (_value)
    {
      var ignored0 = _value;
    })(_23);
  });
  SignalRClient__start$ = (function (unitVar0)
  {
    var _50;
    var _51;
    var conn = LanguagePrimitives__UnboxGeneric$HubConnection_HubConnection_(SignalRClient__serverHub);
    _51 = (conn.createHubProxy("myhub"));
    _50 = (_51.invoke("testUpdating3"));
    (function (value)
    {
      var ignored0 = value;
    })(_50);
    var _62;
    var xx = 1;
    var y = "2";
    var _65;
    var z = _65;
    var a = 5;
    var _67;
    var _conn = LanguagePrimitives__UnboxGeneric$HubConnection_HubConnection_(SignalRClient__serverHub);
    _67 = (_conn.createHubProxy("myhub"));
    _62 = (_67.invoke("functionWith4Args", xx, y, z, a));
    (function (value)
    {
      var ignored0 = value;
    })(_62);
    var intList1 = LanguagePrimitives__UnboxGeneric$List_1_Int32_List_1_Int32_([1, 2, 3]);
    var intList2 = LanguagePrimitives__UnboxGeneric$List_1_Int32_List_1_Int32_([4, 5, 6]);
    var u = (((window._)([1])).union(intList1, intList2));
    var argss = [intList1, intList2];
    var v = (((window._)([1])).union(argss));
    var w = (((window._)([1])).union(intList1, intList2));
    (function (value)
    {
      var ignored0 = value;
    })((SignalRClient__j$("#submit").click((function (_arg1)
    {
      var _121;
      var _122;
      var x = 1;
      var _y = "2";
      var _z = 3;
      var _126;
      var __conn = LanguagePrimitives__UnboxGeneric$HubConnection_HubConnection_(SignalRClient__serverHub);
      _126 = (__conn.createHubProxy("myhub"));
      _122 = (_126.invoke("functionWith3Args", x, _y, _z));
      _121 = (_122.done((function (_x)
      {
        return SignalRClient__log(_x.toString());
      })));
      (function (value)
      {
        var ignored0 = value;
      })(_121);
      var _142;
      var fromClient = LanguagePrimitives__UnboxGeneric$String_String((SignalRClient__j$("#source").val()));
      var _148;
      var ___conn = LanguagePrimitives__UnboxGeneric$HubConnection_HubConnection_(SignalRClient__serverHub);
      _148 = (___conn.createHubProxy("myhub"));
      _142 = (_148.invoke("MyCustomServerFunction", fromClient));
      (function (value)
      {
        var ignored0 = value;
      })(_142);
    }))));
    return SignalRClient__log("##Started!##");
  });
  SignalRClient__signalR = SignalRClient__get_signalR$();
  SignalRClient__proxy = SignalRClient__get_proxy$();
  SignalRClient__log = SignalRClient__get_log$();
  SignalRClient__serverHub = SignalRClient__get_serverHub$();
  SignalRClient__main$()