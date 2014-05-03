// Code from DefinitelyTyped Project. https://github.com/borisyankov/DefinitelyTyped (MIT license)
//   JQueryPromise & JQueryDeferred definitions (c) Microsoft
//   SignalR definitions (c) Boris Yankov https://github.com/borisyankov/
//   Updated by Murat Girgin

/*    
    Copyrights are respective of each contributor listed at the beginning of each definition file.

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

// Simplified JQueryPromise and JQueryDefered, renamed to prevent name potential clashes with jquery
interface IPromise<T> {                                     
    always(...alwaysCallbacks: any[]): IPromise<T>;
    done(...doneCallbacks: any[]): IPromise<T>;
    fail(...failCallbacks: any[]): IPromise<T>;
    progress(...progressCallbacks: any[]): IPromise<T>;
    then<U>(onFulfill: (...values: any[]) => U, onReject?: (...reasons: any[]) => U, onProgress?: (...progression: any[]) => any): IPromise<U>;
}
interface IDeferred<T> extends IPromise<T> {
    always(...alwaysCallbacks: any[]): IDeferred<T>;
    done(...doneCallbacks: any[]): IDeferred<T>;
    fail(...failCallbacks: any[]): IDeferred<T>;
    progress(...progressCallbacks: any[]): IDeferred<T>;
    notify(...args: any[]): IDeferred<T>;
    notifyWith(context: any, ...args: any[]): IDeferred<T>;
    reject(...args: any[]): IDeferred<T>;
    rejectWith(context: any, ...args: any[]): IDeferred<T>;
    resolve(val: T): IDeferred<T>;
    resolve(...args: any[]): IDeferred<T>;
    resolveWith(context: any, ...args: any[]): IDeferred<T>;
    state(): string;
    promise(target?: any): IPromise<T>;
}

interface HubMethod {
    (callback: (data: string) => void);
}

interface SignalREvents {
    onStart: string;
    onStarting: string;
    onReceived: string;
    onError: string;
    onConnectionSlow: string;
    onReconnect: string;
    onStateChanged: string;
    onDisconnect: string;
}

interface SignalRStateChange {
    oldState: number;
    newState: number;
}

interface SignalR {
    events: SignalREvents;
    connectionState: any;
    transports: any;

    hub: HubConnection;
    id: string;
    logging: boolean;
    messageId: string;
    url: string;

    (url: string, queryString?: any, logging?: boolean): SignalR;
    hubConnection(url?: string): SignalR;

    log(msg: string, logging: boolean): void;
    isCrossDomain(url: string): boolean;
    changeState(connection: SignalR, expectedState: number, newState: number): boolean;
    isDisconnecting(connection: SignalR): boolean;

    // createHubProxy(hubName: string): SignalR;

    start(): IPromise<any>;
    start(callback: () => void): IPromise<any>;
    start(settings: ConnectionSettings): IPromise<any>;
    start(settings: ConnectionSettings, callback: () => void): IPromise<any>;

    send(data: string): void;
    stop(async?: boolean, notifyServer?: boolean): void;

    starting(handler: () => void): SignalR;
    received(handler: (data: any) => void): SignalR;
    error(handler: (error: string) => void): SignalR;
    stateChanged(handler: (change: SignalRStateChange) => void): SignalR;
    disconnected(handler: () => void): SignalR;
    connectionSlow(handler: () => void): SignalR;
    sending(handler: () => void): SignalR;
    reconnecting(handler: () => void): SignalR;
    reconnected(handler: () => void): SignalR;
}

interface HubProxy {
    (connection: HubConnection, hubName: string): HubProxy;
    state: any;
    connection: HubConnection;
    hubName: string;
    init(connection: HubConnection, hubName: string): void;
    hasSubscriptions(): boolean;
    on(eventName: string, callback: (...msg) => void): HubProxy;
    off(eventName: string, callback: (msg) => void): HubProxy;
    invoke(methodName: string, ...args: any[]): any; // IDeferred<any>;
}

interface HubConnectionSettings {
    queryString?: string;
    logging?: boolean;
    useDefaultPath?: boolean;
}

interface HubConnection extends SignalR {
    //(url?: string, queryString?: any, logging?: boolean): HubConnection;
    proxies;
    received(callback: (data: { Id; Method; Hub; State; Args; }) => void): HubConnection;
    createHubProxy(hubName: string): HubProxy;
}

interface SignalRfn {
    init(url, qs, logging);
}

interface ConnectionSettings {
    transport? ;
    callback? ;
    waitForPageLoad?: boolean;
    jsonp?: boolean;
}

declare var $: {
    (): any;
    (any): any;
    signalR: SignalR;
    connection: SignalR;
    hubConnection(url?: string, queryString?: any, logging?: boolean): HubConnection;
};