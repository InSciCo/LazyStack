/*
 * Copyright 2017-2017 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"). You may not use this file except in compliance with
 * the License. A copy of the License is located at
 *
 *     http://aws.amazon.com/apache2.0/
 *
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
 * and limitations under the License.
 */
var CognitoHostedUIIdentityProvider;
(function (CognitoHostedUIIdentityProvider) {
    CognitoHostedUIIdentityProvider["Cognito"] = "COGNITO";
    CognitoHostedUIIdentityProvider["Google"] = "Google";
    CognitoHostedUIIdentityProvider["Facebook"] = "Facebook";
    CognitoHostedUIIdentityProvider["Amazon"] = "LoginWithAmazon";
    CognitoHostedUIIdentityProvider["Apple"] = "SignInWithApple";
})(CognitoHostedUIIdentityProvider || (CognitoHostedUIIdentityProvider = {}));
function isFederatedSignInOptions(obj) {
    var keys = ['provider'];
    return obj && !!keys.find(function (k) { return obj.hasOwnProperty(k); });
}
function isFederatedSignInOptionsCustom(obj) {
    var keys = ['customProvider'];
    return obj && !!keys.find(function (k) { return obj.hasOwnProperty(k); });
}
function hasCustomState(obj) {
    var keys = ['customState'];
    return obj && !!keys.find(function (k) { return obj.hasOwnProperty(k); });
}
function isCognitoHostedOpts(oauth) {
    return oauth.redirectSignIn !== undefined;
}
var AuthErrorTypes;
(function (AuthErrorTypes) {
    AuthErrorTypes["NoConfig"] = "noConfig";
    AuthErrorTypes["MissingAuthConfig"] = "missingAuthConfig";
    AuthErrorTypes["EmptyUsername"] = "emptyUsername";
    AuthErrorTypes["InvalidUsername"] = "invalidUsername";
    AuthErrorTypes["EmptyPassword"] = "emptyPassword";
    AuthErrorTypes["EmptyCode"] = "emptyCode";
    AuthErrorTypes["SignUpError"] = "signUpError";
    AuthErrorTypes["NoMFA"] = "noMFA";
    AuthErrorTypes["InvalidMFA"] = "invalidMFA";
    AuthErrorTypes["EmptyChallengeResponse"] = "emptyChallengeResponse";
    AuthErrorTypes["NoUserSession"] = "noUserSession";
    AuthErrorTypes["Default"] = "default";
    AuthErrorTypes["DeviceConfig"] = "deviceConfig";
    AuthErrorTypes["NetworkError"] = "networkError";
})(AuthErrorTypes || (AuthErrorTypes = {}));
function isUsernamePasswordOpts(obj) {
    return !!obj.username;
}
var GRAPHQL_AUTH_MODE;
(function (GRAPHQL_AUTH_MODE) {
    GRAPHQL_AUTH_MODE["API_KEY"] = "API_KEY";
    GRAPHQL_AUTH_MODE["AWS_IAM"] = "AWS_IAM";
    GRAPHQL_AUTH_MODE["OPENID_CONNECT"] = "OPENID_CONNECT";
    GRAPHQL_AUTH_MODE["AMAZON_COGNITO_USER_POOLS"] = "AMAZON_COGNITO_USER_POOLS";
    GRAPHQL_AUTH_MODE["AWS_LAMBDA"] = "AWS_LAMBDA";
})(GRAPHQL_AUTH_MODE || (GRAPHQL_AUTH_MODE = {}));

/*
 * Copyright 2017-2019 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"). You may not use this file except in compliance with
 * the License. A copy of the License is located at
 *
 *     http://aws.amazon.com/apache2.0/
 *
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
 * and limitations under the License.
 */
var AWS_CLOUDWATCH_CATEGORY = 'Logging';

/*
 * Copyright 2017-2019 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"). You may not use this file except in compliance with
 * the License. A copy of the License is located at
 *
 *     http://aws.amazon.com/apache2.0/
 *
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
 * and limitations under the License.
 */
var __values$1 = (undefined && undefined.__values) || function(o) {
    var s = typeof Symbol === "function" && Symbol.iterator, m = s && o[s], i = 0;
    if (m) return m.call(o);
    if (o && typeof o.length === "number") return {
        next: function () {
            if (o && i >= o.length) o = void 0;
            return { value: o && o[i++], done: !o };
        }
    };
    throw new TypeError(s ? "Object is not iterable." : "Symbol.iterator is not defined.");
};
var __read$7 = (undefined && undefined.__read) || function (o, n) {
    var m = typeof Symbol === "function" && o[Symbol.iterator];
    if (!m) return o;
    var i = m.call(o), r, ar = [], e;
    try {
        while ((n === void 0 || n-- > 0) && !(r = i.next()).done) ar.push(r.value);
    }
    catch (error) { e = { error: error }; }
    finally {
        try {
            if (r && !r.done && (m = i["return"])) m.call(i);
        }
        finally { if (e) throw e.error; }
    }
    return ar;
};
var __spread$3 = (undefined && undefined.__spread) || function () {
    for (var ar = [], i = 0; i < arguments.length; i++) ar = ar.concat(__read$7(arguments[i]));
    return ar;
};
var LOG_LEVELS = {
    VERBOSE: 1,
    DEBUG: 2,
    INFO: 3,
    WARN: 4,
    ERROR: 5,
};
var LOG_TYPE;
(function (LOG_TYPE) {
    LOG_TYPE["DEBUG"] = "DEBUG";
    LOG_TYPE["ERROR"] = "ERROR";
    LOG_TYPE["INFO"] = "INFO";
    LOG_TYPE["WARN"] = "WARN";
    LOG_TYPE["VERBOSE"] = "VERBOSE";
})(LOG_TYPE || (LOG_TYPE = {}));
/**
 * Write logs
 * @class Logger
 */
var ConsoleLogger = /** @class */ (function () {
    /**
     * @constructor
     * @param {string} name - Name of the logger
     */
    function ConsoleLogger(name, level) {
        if (level === void 0) { level = LOG_TYPE.WARN; }
        this.name = name;
        this.level = level;
        this._pluggables = [];
    }
    ConsoleLogger.prototype._padding = function (n) {
        return n < 10 ? '0' + n : '' + n;
    };
    ConsoleLogger.prototype._ts = function () {
        var dt = new Date();
        return ([this._padding(dt.getMinutes()), this._padding(dt.getSeconds())].join(':') +
            '.' +
            dt.getMilliseconds());
    };
    ConsoleLogger.prototype.configure = function (config) {
        if (!config)
            return this._config;
        this._config = config;
        return this._config;
    };
    /**
     * Write log
     * @method
     * @memeberof Logger
     * @param {LOG_TYPE|string} type - log type, default INFO
     * @param {string|object} msg - Logging message or object
     */
    ConsoleLogger.prototype._log = function (type) {
        var e_1, _a;
        var msg = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            msg[_i - 1] = arguments[_i];
        }
        var logger_level_name = this.level;
        if (ConsoleLogger.LOG_LEVEL) {
            logger_level_name = ConsoleLogger.LOG_LEVEL;
        }
        if (typeof window !== 'undefined' && window.LOG_LEVEL) {
            logger_level_name = window.LOG_LEVEL;
        }
        var logger_level = LOG_LEVELS[logger_level_name];
        var type_level = LOG_LEVELS[type];
        if (!(type_level >= logger_level)) {
            // Do nothing if type is not greater than or equal to logger level (handle undefined)
            return;
        }
        var log = console.log.bind(console);
        if (type === LOG_TYPE.ERROR && console.error) {
            log = console.error.bind(console);
        }
        if (type === LOG_TYPE.WARN && console.warn) {
            log = console.warn.bind(console);
        }
        var prefix = "[" + type + "] " + this._ts() + " " + this.name;
        var message = '';
        if (msg.length === 1 && typeof msg[0] === 'string') {
            message = prefix + " - " + msg[0];
            log(message);
        }
        else if (msg.length === 1) {
            message = prefix + " " + msg[0];
            log(prefix, msg[0]);
        }
        else if (typeof msg[0] === 'string') {
            var obj = msg.slice(1);
            if (obj.length === 1) {
                obj = obj[0];
            }
            message = prefix + " - " + msg[0] + " " + obj;
            log(prefix + " - " + msg[0], obj);
        }
        else {
            message = prefix + " " + msg;
            log(prefix, msg);
        }
        try {
            for (var _b = __values$1(this._pluggables), _c = _b.next(); !_c.done; _c = _b.next()) {
                var plugin = _c.value;
                var logEvent = { message: message, timestamp: Date.now() };
                plugin.pushLogs([logEvent]);
            }
        }
        catch (e_1_1) { e_1 = { error: e_1_1 }; }
        finally {
            try {
                if (_c && !_c.done && (_a = _b.return)) _a.call(_b);
            }
            finally { if (e_1) throw e_1.error; }
        }
    };
    /**
     * Write General log. Default to INFO
     * @method
     * @memeberof Logger
     * @param {string|object} msg - Logging message or object
     */
    ConsoleLogger.prototype.log = function () {
        var msg = [];
        for (var _i = 0; _i < arguments.length; _i++) {
            msg[_i] = arguments[_i];
        }
        this._log.apply(this, __spread$3([LOG_TYPE.INFO], msg));
    };
    /**
     * Write INFO log
     * @method
     * @memeberof Logger
     * @param {string|object} msg - Logging message or object
     */
    ConsoleLogger.prototype.info = function () {
        var msg = [];
        for (var _i = 0; _i < arguments.length; _i++) {
            msg[_i] = arguments[_i];
        }
        this._log.apply(this, __spread$3([LOG_TYPE.INFO], msg));
    };
    /**
     * Write WARN log
     * @method
     * @memeberof Logger
     * @param {string|object} msg - Logging message or object
     */
    ConsoleLogger.prototype.warn = function () {
        var msg = [];
        for (var _i = 0; _i < arguments.length; _i++) {
            msg[_i] = arguments[_i];
        }
        this._log.apply(this, __spread$3([LOG_TYPE.WARN], msg));
    };
    /**
     * Write ERROR log
     * @method
     * @memeberof Logger
     * @param {string|object} msg - Logging message or object
     */
    ConsoleLogger.prototype.error = function () {
        var msg = [];
        for (var _i = 0; _i < arguments.length; _i++) {
            msg[_i] = arguments[_i];
        }
        this._log.apply(this, __spread$3([LOG_TYPE.ERROR], msg));
    };
    /**
     * Write DEBUG log
     * @method
     * @memeberof Logger
     * @param {string|object} msg - Logging message or object
     */
    ConsoleLogger.prototype.debug = function () {
        var msg = [];
        for (var _i = 0; _i < arguments.length; _i++) {
            msg[_i] = arguments[_i];
        }
        this._log.apply(this, __spread$3([LOG_TYPE.DEBUG], msg));
    };
    /**
     * Write VERBOSE log
     * @method
     * @memeberof Logger
     * @param {string|object} msg - Logging message or object
     */
    ConsoleLogger.prototype.verbose = function () {
        var msg = [];
        for (var _i = 0; _i < arguments.length; _i++) {
            msg[_i] = arguments[_i];
        }
        this._log.apply(this, __spread$3([LOG_TYPE.VERBOSE], msg));
    };
    ConsoleLogger.prototype.addPluggable = function (pluggable) {
        if (pluggable && pluggable.getCategoryName() === AWS_CLOUDWATCH_CATEGORY) {
            this._pluggables.push(pluggable);
            pluggable.configure(this._config);
        }
    };
    ConsoleLogger.prototype.listPluggables = function () {
        return this._pluggables;
    };
    ConsoleLogger.LOG_LEVEL = null;
    return ConsoleLogger;
}());

var __read$6 = (undefined && undefined.__read) || function (o, n) {
    var m = typeof Symbol === "function" && o[Symbol.iterator];
    if (!m) return o;
    var i = m.call(o), r, ar = [], e;
    try {
        while ((n === void 0 || n-- > 0) && !(r = i.next()).done) ar.push(r.value);
    }
    catch (error) { e = { error: error }; }
    finally {
        try {
            if (r && !r.done && (m = i["return"])) m.call(i);
        }
        finally { if (e) throw e.error; }
    }
    return ar;
};
var logger$c = new ConsoleLogger('Amplify');
var AmplifyClass = /** @class */ (function () {
    function AmplifyClass() {
        // Everything that is `register`ed is tracked here
        this._components = [];
        this._config = {};
        // All modules (with `getModuleName()`) are stored here for dependency injection
        this._modules = {};
        // for backward compatibility to avoid breaking change
        // if someone is using like Amplify.Auth
        this.Auth = null;
        this.Analytics = null;
        this.API = null;
        this.Credentials = null;
        this.Storage = null;
        this.I18n = null;
        this.Cache = null;
        this.PubSub = null;
        this.Interactions = null;
        this.Pushnotification = null;
        this.UI = null;
        this.XR = null;
        this.Predictions = null;
        this.DataStore = null;
        this.Geo = null;
        this.Logger = ConsoleLogger;
        this.ServiceWorker = null;
    }
    AmplifyClass.prototype.register = function (comp) {
        logger$c.debug('component registered in amplify', comp);
        this._components.push(comp);
        if (typeof comp.getModuleName === 'function') {
            this._modules[comp.getModuleName()] = comp;
            this[comp.getModuleName()] = comp;
        }
        else {
            logger$c.debug('no getModuleName method for component', comp);
        }
        // Finally configure this new component(category) loaded
        // With the new modularization changes in Amplify V3, all the Amplify
        // component are not loaded/registered right away but when they are
        // imported (and hence instantiated) in the client's app. This ensures
        // that all new components imported get correctly configured with the
        // configuration that Amplify.configure() was called with.
        comp.configure(this._config);
    };
    AmplifyClass.prototype.configure = function (config) {
        var _this = this;
        if (!config)
            return this._config;
        this._config = Object.assign(this._config, config);
        logger$c.debug('amplify config', this._config);
        // Dependency Injection via property-setting.
        // This avoids introducing a public method/interface/setter that's difficult to remove later.
        // Plus, it reduces `if` statements within the `constructor` and `configure` of each module
        Object.entries(this._modules).forEach(function (_a) {
            var _b = __read$6(_a, 2); _b[0]; var comp = _b[1];
            // e.g. Auth.*
            Object.keys(comp).forEach(function (property) {
                // e.g. Auth["Credentials"] = this._modules["Credentials"] when set
                if (_this._modules[property]) {
                    comp[property] = _this._modules[property];
                }
            });
        });
        this._components.map(function (comp) {
            comp.configure(_this._config);
        });
        return this._config;
    };
    AmplifyClass.prototype.addPluggable = function (pluggable) {
        if (pluggable &&
            pluggable['getCategory'] &&
            typeof pluggable['getCategory'] === 'function') {
            this._components.map(function (comp) {
                if (comp['addPluggable'] &&
                    typeof comp['addPluggable'] === 'function') {
                    comp.addPluggable(pluggable);
                }
            });
        }
    };
    return AmplifyClass;
}());
var Amplify = new AmplifyClass();

// generated by genversion
var version$3 = '4.5.7';

/*
 * Copyright 2017-2017 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"). You may not use this file except in compliance with
 * the License. A copy of the License is located at
 *
 *     http://aws.amazon.com/apache2.0/
 *
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
 * and limitations under the License.
 */
var BASE_USER_AGENT$1 = "aws-amplify/" + version$3;
var Platform$1 = {
    userAgent: BASE_USER_AGENT$1 + " js",
    product: '',
    navigator: null,
    isReactNative: false,
};
if (typeof navigator !== 'undefined' && navigator.product) {
    Platform$1.product = navigator.product || '';
    Platform$1.navigator = navigator || null;
    switch (navigator.product) {
        case 'ReactNative':
            Platform$1.userAgent = BASE_USER_AGENT$1 + " react-native";
            Platform$1.isReactNative = true;
            break;
        default:
            Platform$1.userAgent = BASE_USER_AGENT$1 + " js";
            Platform$1.isReactNative = false;
            break;
    }
}
var getAmplifyUserAgent = function () {
    return Platform$1.userAgent;
};

/*
 * Copyright 2017-2017 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"). You may not use this file except in compliance with
 * the License. A copy of the License is located at
 *
 *     http://aws.amazon.com/apache2.0/
 *
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
 * and limitations under the License.
 */
var __assign$7 = (undefined && undefined.__assign) || function () {
    __assign$7 = Object.assign || function(t) {
        for (var s, i = 1, n = arguments.length; i < n; i++) {
            s = arguments[i];
            for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
                t[p] = s[p];
        }
        return t;
    };
    return __assign$7.apply(this, arguments);
};
var __read$5 = (undefined && undefined.__read) || function (o, n) {
    var m = typeof Symbol === "function" && o[Symbol.iterator];
    if (!m) return o;
    var i = m.call(o), r, ar = [], e;
    try {
        while ((n === void 0 || n-- > 0) && !(r = i.next()).done) ar.push(r.value);
    }
    catch (error) { e = { error: error }; }
    finally {
        try {
            if (r && !r.done && (m = i["return"])) m.call(i);
        }
        finally { if (e) throw e.error; }
    }
    return ar;
};
var __spread$2 = (undefined && undefined.__spread) || function () {
    for (var ar = [], i = 0; i < arguments.length; i++) ar = ar.concat(__read$5(arguments[i]));
    return ar;
};
var logger$b = new ConsoleLogger('Hub');
var AMPLIFY_SYMBOL$2 = (typeof Symbol !== 'undefined' &&
    typeof Symbol.for === 'function'
    ? Symbol.for('amplify_default')
    : '@@amplify_default');
function isLegacyCallback(callback) {
    return callback.onHubCapsule !== undefined;
}
var HubClass = /** @class */ (function () {
    function HubClass(name) {
        this.listeners = [];
        this.patterns = [];
        this.protectedChannels = [
            'core',
            'auth',
            'api',
            'analytics',
            'interactions',
            'pubsub',
            'storage',
            'ui',
            'xr',
        ];
        this.name = name;
    }
    // Note - Need to pass channel as a reference for removal to work and not anonymous function
    HubClass.prototype.remove = function (channel, listener) {
        if (channel instanceof RegExp) {
            var pattern_1 = this.patterns.find(function (_a) {
                var pattern = _a.pattern;
                return pattern.source === channel.source;
            });
            if (!pattern_1) {
                logger$b.warn("No listeners for " + channel);
                return;
            }
            this.patterns = __spread$2(this.patterns.filter(function (x) { return x !== pattern_1; }));
        }
        else {
            var holder = this.listeners[channel];
            if (!holder) {
                logger$b.warn("No listeners for " + channel);
                return;
            }
            this.listeners[channel] = __spread$2(holder.filter(function (_a) {
                var callback = _a.callback;
                return callback !== listener;
            }));
        }
    };
    HubClass.prototype.dispatch = function (channel, payload, source, ampSymbol) {
        if (source === void 0) { source = ''; }
        if (this.protectedChannels.indexOf(channel) > -1) {
            var hasAccess = ampSymbol === AMPLIFY_SYMBOL$2;
            if (!hasAccess) {
                logger$b.warn("WARNING: " + channel + " is protected and dispatching on it can have unintended consequences");
            }
        }
        var capsule = {
            channel: channel,
            payload: __assign$7({}, payload),
            source: source,
            patternInfo: [],
        };
        try {
            this._toListeners(capsule);
        }
        catch (e) {
            logger$b.error(e);
        }
    };
    HubClass.prototype.listen = function (channel, callback, listenerName) {
        var _this = this;
        if (listenerName === void 0) { listenerName = 'noname'; }
        var cb;
        // Check for legacy onHubCapsule callback for backwards compatability
        if (isLegacyCallback(callback)) {
            logger$b.warn("WARNING onHubCapsule is Deprecated. Please pass in a callback.");
            cb = callback.onHubCapsule.bind(callback);
        }
        else if (typeof callback !== 'function') {
            throw new Error('No callback supplied to Hub');
        }
        else {
            cb = callback;
        }
        if (channel instanceof RegExp) {
            this.patterns.push({
                pattern: channel,
                callback: cb,
            });
        }
        else {
            var holder = this.listeners[channel];
            if (!holder) {
                holder = [];
                this.listeners[channel] = holder;
            }
            holder.push({
                name: listenerName,
                callback: cb,
            });
        }
        return function () {
            _this.remove(channel, cb);
        };
    };
    HubClass.prototype._toListeners = function (capsule) {
        var channel = capsule.channel, payload = capsule.payload;
        var holder = this.listeners[channel];
        if (holder) {
            holder.forEach(function (listener) {
                logger$b.debug("Dispatching to " + channel + " with ", payload);
                try {
                    listener.callback(capsule);
                }
                catch (e) {
                    logger$b.error(e);
                }
            });
        }
        if (this.patterns.length > 0) {
            if (!payload.message) {
                logger$b.warn("Cannot perform pattern matching without a message key");
                return;
            }
            var payloadStr_1 = payload.message;
            this.patterns.forEach(function (pattern) {
                var match = payloadStr_1.match(pattern.pattern);
                if (match) {
                    var _a = __read$5(match), groups = _a.slice(1);
                    var dispatchingCapsule = __assign$7(__assign$7({}, capsule), { patternInfo: groups });
                    try {
                        pattern.callback(dispatchingCapsule);
                    }
                    catch (e) {
                        logger$b.error(e);
                    }
                }
            });
        }
    };
    return HubClass;
}());
/*We export a __default__ instance of HubClass to use it as a
pseudo Singleton for the main messaging bus, however you can still create
your own instance of HubClass() for a separate "private bus" of events.*/
var Hub = new HubClass('__default__');

/*
 * Copyright 2017-2017 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"). You may not use this file except in compliance with
 * the License. A copy of the License is located at
 *
 *     http://aws.amazon.com/apache2.0/
 *
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
 * and limitations under the License.
 */
var logger$a = new ConsoleLogger('I18n');
/**
 * Language transition class
 */
var I18n$1 = /** @class */ (function () {
    /**
     * @constructor
     * Initialize with configurations
     * @param {Object} options
     */
    function I18n(options) {
        /**
         * @private
         */
        this._options = null;
        /**
         * @private
         */
        this._lang = null;
        /**
         * @private
         */
        this._dict = {};
        this._options = Object.assign({}, options);
        this._lang = this._options.language;
        if (!this._lang &&
            typeof window !== 'undefined' &&
            window &&
            window.navigator) {
            this._lang = window.navigator.language;
        }
        logger$a.debug(this._lang);
    }
    /**
     * @method
     * Explicitly setting language
     * @param {String} lang
     */
    I18n.prototype.setLanguage = function (lang) {
        this._lang = lang;
    };
    /**
     * @method
     * Get value
     * @param {String} key
     * @param {String} defVal - Default value
     */
    I18n.prototype.get = function (key, defVal) {
        if (defVal === void 0) { defVal = undefined; }
        if (!this._lang) {
            return typeof defVal !== 'undefined' ? defVal : key;
        }
        var lang = this._lang;
        var val = this.getByLanguage(key, lang);
        if (val) {
            return val;
        }
        if (lang.indexOf('-') > 0) {
            val = this.getByLanguage(key, lang.split('-')[0]);
        }
        if (val) {
            return val;
        }
        return typeof defVal !== 'undefined' ? defVal : key;
    };
    /**
     * @method
     * Get value according to specified language
     * @param {String} key
     * @param {String} language - Specified langurage to be used
     * @param {String} defVal - Default value
     */
    I18n.prototype.getByLanguage = function (key, language, defVal) {
        if (defVal === void 0) { defVal = null; }
        if (!language) {
            return defVal;
        }
        var lang_dict = this._dict[language];
        if (!lang_dict) {
            return defVal;
        }
        return lang_dict[key];
    };
    /**
     * @method
     * Add vocabularies for one language
     * @param {String} language - Language of the dictionary
     * @param {Object} vocabularies - Object that has key-value as dictionary entry
     */
    I18n.prototype.putVocabulariesForLanguage = function (language, vocabularies) {
        var lang_dict = this._dict[language];
        if (!lang_dict) {
            lang_dict = this._dict[language] = {};
        }
        Object.assign(lang_dict, vocabularies);
    };
    /**
     * @method
     * Add vocabularies for one language
     * @param {Object} vocabularies - Object that has language as key,
     *                                vocabularies of each language as value
     */
    I18n.prototype.putVocabularies = function (vocabularies) {
        var _this = this;
        Object.keys(vocabularies).map(function (key) {
            _this.putVocabulariesForLanguage(key, vocabularies[key]);
        });
    };
    return I18n;
}());

/*
 * Copyright 2017-2017 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"). You may not use this file except in compliance with
 * the License. A copy of the License is located at
 *
 *     http://aws.amazon.com/apache2.0/
 *
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
 * and limitations under the License.
 */
var logger$9 = new ConsoleLogger('I18n');
var _config = null;
var _i18n = null;
/**
 * Export I18n APIs
 */
var I18n = /** @class */ (function () {
    function I18n() {
    }
    /**
     * @static
     * @method
     * Configure I18n part
     * @param {Object} config - Configuration of the I18n
     */
    I18n.configure = function (config) {
        logger$9.debug('configure I18n');
        if (!config) {
            return _config;
        }
        _config = Object.assign({}, _config, config.I18n || config);
        I18n.createInstance();
        return _config;
    };
    I18n.getModuleName = function () {
        return 'I18n';
    };
    /**
     * @static
     * @method
     * Create an instance of I18n for the library
     */
    I18n.createInstance = function () {
        logger$9.debug('create I18n instance');
        if (_i18n) {
            return;
        }
        _i18n = new I18n$1(_config);
    };
    /**
     * @static @method
     * Explicitly setting language
     * @param {String} lang
     */
    I18n.setLanguage = function (lang) {
        I18n.checkConfig();
        return _i18n.setLanguage(lang);
    };
    /**
     * @static @method
     * Get value
     * @param {String} key
     * @param {String} defVal - Default value
     */
    I18n.get = function (key, defVal) {
        if (!I18n.checkConfig()) {
            return typeof defVal === 'undefined' ? key : defVal;
        }
        return _i18n.get(key, defVal);
    };
    /**
     * @static
     * @method
     * Add vocabularies for one language
     * @param {String} langurage - Language of the dictionary
     * @param {Object} vocabularies - Object that has key-value as dictionary entry
     */
    I18n.putVocabulariesForLanguage = function (language, vocabularies) {
        I18n.checkConfig();
        return _i18n.putVocabulariesForLanguage(language, vocabularies);
    };
    /**
     * @static
     * @method
     * Add vocabularies for one language
     * @param {Object} vocabularies - Object that has language as key,
     *                                vocabularies of each language as value
     */
    I18n.putVocabularies = function (vocabularies) {
        I18n.checkConfig();
        return _i18n.putVocabularies(vocabularies);
    };
    I18n.checkConfig = function () {
        if (!_i18n) {
            _i18n = new I18n$1(_config);
        }
        return true;
    };
    return I18n;
}());
Amplify.register(I18n);

/* SNOWPACK PROCESS POLYFILL (based on https://github.com/calvinmetcalf/node-process-es6) */
function defaultSetTimout() {
    throw new Error('setTimeout has not been defined');
}
function defaultClearTimeout () {
    throw new Error('clearTimeout has not been defined');
}
var cachedSetTimeout = defaultSetTimout;
var cachedClearTimeout = defaultClearTimeout;
var globalContext;
if (typeof window !== 'undefined') {
    globalContext = window;
} else if (typeof self !== 'undefined') {
    globalContext = self;
} else {
    globalContext = {};
}
if (typeof globalContext.setTimeout === 'function') {
    cachedSetTimeout = setTimeout;
}
if (typeof globalContext.clearTimeout === 'function') {
    cachedClearTimeout = clearTimeout;
}

function runTimeout(fun) {
    if (cachedSetTimeout === setTimeout) {
        //normal enviroments in sane situations
        return setTimeout(fun, 0);
    }
    // if setTimeout wasn't available but was latter defined
    if ((cachedSetTimeout === defaultSetTimout || !cachedSetTimeout) && setTimeout) {
        cachedSetTimeout = setTimeout;
        return setTimeout(fun, 0);
    }
    try {
        // when when somebody has screwed with setTimeout but no I.E. maddness
        return cachedSetTimeout(fun, 0);
    } catch(e){
        try {
            // When we are in I.E. but the script has been evaled so I.E. doesn't trust the global object when called normally
            return cachedSetTimeout.call(null, fun, 0);
        } catch(e){
            // same as above but when it's a version of I.E. that must have the global object for 'this', hopfully our context correct otherwise it will throw a global error
            return cachedSetTimeout.call(this, fun, 0);
        }
    }


}
function runClearTimeout(marker) {
    if (cachedClearTimeout === clearTimeout) {
        //normal enviroments in sane situations
        return clearTimeout(marker);
    }
    // if clearTimeout wasn't available but was latter defined
    if ((cachedClearTimeout === defaultClearTimeout || !cachedClearTimeout) && clearTimeout) {
        cachedClearTimeout = clearTimeout;
        return clearTimeout(marker);
    }
    try {
        // when when somebody has screwed with setTimeout but no I.E. maddness
        return cachedClearTimeout(marker);
    } catch (e){
        try {
            // When we are in I.E. but the script has been evaled so I.E. doesn't  trust the global object when called normally
            return cachedClearTimeout.call(null, marker);
        } catch (e){
            // same as above but when it's a version of I.E. that must have the global object for 'this', hopfully our context correct otherwise it will throw a global error.
            // Some versions of I.E. have different rules for clearTimeout vs setTimeout
            return cachedClearTimeout.call(this, marker);
        }
    }



}
var queue = [];
var draining = false;
var currentQueue;
var queueIndex = -1;

function cleanUpNextTick() {
    if (!draining || !currentQueue) {
        return;
    }
    draining = false;
    if (currentQueue.length) {
        queue = currentQueue.concat(queue);
    } else {
        queueIndex = -1;
    }
    if (queue.length) {
        drainQueue();
    }
}

function drainQueue() {
    if (draining) {
        return;
    }
    var timeout = runTimeout(cleanUpNextTick);
    draining = true;

    var len = queue.length;
    while(len) {
        currentQueue = queue;
        queue = [];
        while (++queueIndex < len) {
            if (currentQueue) {
                currentQueue[queueIndex].run();
            }
        }
        queueIndex = -1;
        len = queue.length;
    }
    currentQueue = null;
    draining = false;
    runClearTimeout(timeout);
}
function nextTick(fun) {
    var args = new Array(arguments.length - 1);
    if (arguments.length > 1) {
        for (var i = 1; i < arguments.length; i++) {
            args[i - 1] = arguments[i];
        }
    }
    queue.push(new Item(fun, args));
    if (queue.length === 1 && !draining) {
        runTimeout(drainQueue);
    }
}
// v8 likes predictible objects
function Item(fun, array) {
    this.fun = fun;
    this.array = array;
}
Item.prototype.run = function () {
    this.fun.apply(null, this.array);
};
var title = 'browser';
var platform = 'browser';
var browser$1 = true;
var argv = [];
var version$2 = ''; // empty string to avoid regexp issues
var versions = {};
var release = {};
var config = {};

function noop() {}

var on = noop;
var addListener = noop;
var once = noop;
var off = noop;
var removeListener = noop;
var removeAllListeners = noop;
var emit = noop;

function binding(name) {
    throw new Error('process.binding is not supported');
}

function cwd () { return '/' }
function chdir (dir) {
    throw new Error('process.chdir is not supported');
}function umask() { return 0; }

// from https://github.com/kumavis/browser-process-hrtime/blob/master/index.js
var performance = globalContext.performance || {};
var performanceNow =
  performance.now        ||
  performance.mozNow     ||
  performance.msNow      ||
  performance.oNow       ||
  performance.webkitNow  ||
  function(){ return (new Date()).getTime() };

// generate timestamp or delta
// see http://nodejs.org/api/process.html#process_process_hrtime
function hrtime(previousTimestamp){
  var clocktime = performanceNow.call(performance)*1e-3;
  var seconds = Math.floor(clocktime);
  var nanoseconds = Math.floor((clocktime%1)*1e9);
  if (previousTimestamp) {
    seconds = seconds - previousTimestamp[0];
    nanoseconds = nanoseconds - previousTimestamp[1];
    if (nanoseconds<0) {
      seconds--;
      nanoseconds += 1e9;
    }
  }
  return [seconds,nanoseconds]
}

var startTime = new Date();
function uptime() {
  var currentTime = new Date();
  var dif = currentTime - startTime;
  return dif / 1000;
}

var process = {
  nextTick: nextTick,
  title: title,
  browser: browser$1,
  env: {"NODE_ENV":"production"},
  argv: argv,
  version: version$2,
  versions: versions,
  on: on,
  addListener: addListener,
  once: once,
  off: off,
  removeListener: removeListener,
  removeAllListeners: removeAllListeners,
  emit: emit,
  binding: binding,
  cwd: cwd,
  chdir: chdir,
  umask: umask,
  hrtime: hrtime,
  platform: platform,
  release: release,
  config: config,
  uptime: uptime
};

/*
 * Copyright 2017-2017 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"). You may not use this file except in compliance with
 * the License. A copy of the License is located at
 *
 *     http://aws.amazon.com/apache2.0/
 *
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
 * and limitations under the License.
 */
var MIME_MAP = [
    { type: 'text/plain', ext: 'txt' },
    { type: 'text/html', ext: 'html' },
    { type: 'text/javascript', ext: 'js' },
    { type: 'text/css', ext: 'css' },
    { type: 'text/csv', ext: 'csv' },
    { type: 'text/yaml', ext: 'yml' },
    { type: 'text/yaml', ext: 'yaml' },
    { type: 'text/calendar', ext: 'ics' },
    { type: 'text/calendar', ext: 'ical' },
    { type: 'image/apng', ext: 'apng' },
    { type: 'image/bmp', ext: 'bmp' },
    { type: 'image/gif', ext: 'gif' },
    { type: 'image/x-icon', ext: 'ico' },
    { type: 'image/x-icon', ext: 'cur' },
    { type: 'image/jpeg', ext: 'jpg' },
    { type: 'image/jpeg', ext: 'jpeg' },
    { type: 'image/jpeg', ext: 'jfif' },
    { type: 'image/jpeg', ext: 'pjp' },
    { type: 'image/jpeg', ext: 'pjpeg' },
    { type: 'image/png', ext: 'png' },
    { type: 'image/svg+xml', ext: 'svg' },
    { type: 'image/tiff', ext: 'tif' },
    { type: 'image/tiff', ext: 'tiff' },
    { type: 'image/webp', ext: 'webp' },
    { type: 'application/json', ext: 'json' },
    { type: 'application/xml', ext: 'xml' },
    { type: 'application/x-sh', ext: 'sh' },
    { type: 'application/zip', ext: 'zip' },
    { type: 'application/x-rar-compressed', ext: 'rar' },
    { type: 'application/x-tar', ext: 'tar' },
    { type: 'application/x-bzip', ext: 'bz' },
    { type: 'application/x-bzip2', ext: 'bz2' },
    { type: 'application/pdf', ext: 'pdf' },
    { type: 'application/java-archive', ext: 'jar' },
    { type: 'application/msword', ext: 'doc' },
    { type: 'application/vnd.ms-excel', ext: 'xls' },
    { type: 'application/vnd.ms-excel', ext: 'xlsx' },
    { type: 'message/rfc822', ext: 'eml' },
];
var isEmpty = function (obj) {
    if (obj === void 0) { obj = {}; }
    return Object.keys(obj).length === 0;
};
var sortByField = function (list, field, dir) {
    if (!list || !list.sort) {
        return false;
    }
    var dirX = dir && dir === 'desc' ? -1 : 1;
    list.sort(function (a, b) {
        var a_val = a[field];
        var b_val = b[field];
        if (typeof b_val === 'undefined') {
            return typeof a_val === 'undefined' ? 0 : 1 * dirX;
        }
        if (typeof a_val === 'undefined') {
            return -1 * dirX;
        }
        if (a_val < b_val) {
            return -1 * dirX;
        }
        if (a_val > b_val) {
            return 1 * dirX;
        }
        return 0;
    });
    return true;
};
var objectLessAttributes = function (obj, less) {
    var ret = Object.assign({}, obj);
    if (less) {
        if (typeof less === 'string') {
            delete ret[less];
        }
        else {
            less.forEach(function (attr) {
                delete ret[attr];
            });
        }
    }
    return ret;
};
var filenameToContentType = function (filename, defVal) {
    if (defVal === void 0) { defVal = 'application/octet-stream'; }
    var name = filename.toLowerCase();
    var filtered = MIME_MAP.filter(function (mime) { return name.endsWith('.' + mime.ext); });
    return filtered.length > 0 ? filtered[0].type : defVal;
};
var isTextFile = function (contentType) {
    var type = contentType.toLowerCase();
    if (type.startsWith('text/')) {
        return true;
    }
    return ('application/json' === type ||
        'application/xml' === type ||
        'application/sh' === type);
};
var generateRandomString = function () {
    var result = '';
    var chars = '0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ';
    for (var i = 32; i > 0; i -= 1) {
        result += chars[Math.floor(Math.random() * chars.length)];
    }
    return result;
};
var makeQuerablePromise = function (promise) {
    if (promise.isResolved)
        return promise;
    var isPending = true;
    var isRejected = false;
    var isFullfilled = false;
    var result = promise.then(function (data) {
        isFullfilled = true;
        isPending = false;
        return data;
    }, function (e) {
        isRejected = true;
        isPending = false;
        throw e;
    });
    result.isFullfilled = function () { return isFullfilled; };
    result.isPending = function () { return isPending; };
    result.isRejected = function () { return isRejected; };
    return result;
};
var isWebWorker = function () {
    if (typeof self === 'undefined') {
        return false;
    }
    var selfContext = self;
    return typeof selfContext.WorkerGlobalScope !== 'undefined' &&
        self instanceof selfContext.WorkerGlobalScope;
};
var browserOrNode = function () {
    var isBrowser = typeof window !== 'undefined' && typeof window.document !== 'undefined';
    var isNode = typeof process !== 'undefined' &&
        process.versions != null &&
        process.versions.node != null;
    return {
        isBrowser: isBrowser,
        isNode: isNode,
    };
};
/**
 * transfer the first letter of the keys to lowercase
 * @param {Object} obj - the object need to be transferred
 * @param {Array} whiteListForItself - whitelist itself from being transferred
 * @param {Array} whiteListForChildren - whitelist its children keys from being transferred
 */
var transferKeyToLowerCase = function (obj, whiteListForItself, whiteListForChildren) {
    if (whiteListForItself === void 0) { whiteListForItself = []; }
    if (whiteListForChildren === void 0) { whiteListForChildren = []; }
    if (!isStrictObject(obj))
        return obj;
    var ret = {};
    for (var key in obj) {
        if (obj.hasOwnProperty(key)) {
            var transferedKey = whiteListForItself.includes(key)
                ? key
                : key[0].toLowerCase() + key.slice(1);
            ret[transferedKey] = whiteListForChildren.includes(key)
                ? obj[key]
                : transferKeyToLowerCase(obj[key], whiteListForItself, whiteListForChildren);
        }
    }
    return ret;
};
/**
 * transfer the first letter of the keys to lowercase
 * @param {Object} obj - the object need to be transferred
 * @param {Array} whiteListForItself - whitelist itself from being transferred
 * @param {Array} whiteListForChildren - whitelist its children keys from being transferred
 */
var transferKeyToUpperCase = function (obj, whiteListForItself, whiteListForChildren) {
    if (whiteListForItself === void 0) { whiteListForItself = []; }
    if (whiteListForChildren === void 0) { whiteListForChildren = []; }
    if (!isStrictObject(obj))
        return obj;
    var ret = {};
    for (var key in obj) {
        if (obj.hasOwnProperty(key)) {
            var transferredKey = whiteListForItself.includes(key)
                ? key
                : key[0].toUpperCase() + key.slice(1);
            ret[transferredKey] = whiteListForChildren.includes(key)
                ? obj[key]
                : transferKeyToUpperCase(obj[key], whiteListForItself, whiteListForChildren);
        }
    }
    return ret;
};
/**
 * Return true if the object is a strict object
 * which means it's not Array, Function, Number, String, Boolean or Null
 * @param obj the Object
 */
var isStrictObject = function (obj) {
    return (obj instanceof Object &&
        !(obj instanceof Array) &&
        !(obj instanceof Function) &&
        !(obj instanceof Number) &&
        !(obj instanceof String) &&
        !(obj instanceof Boolean));
};
/**
 * @deprecated use per-function imports
 */
var JS = /** @class */ (function () {
    function JS() {
    }
    JS.isEmpty = isEmpty;
    JS.sortByField = sortByField;
    JS.objectLessAttributes = objectLessAttributes;
    JS.filenameToContentType = filenameToContentType;
    JS.isTextFile = isTextFile;
    JS.generateRandomString = generateRandomString;
    JS.makeQuerablePromise = makeQuerablePromise;
    JS.isWebWorker = isWebWorker;
    JS.browserOrNode = browserOrNode;
    JS.transferKeyToLowerCase = transferKeyToLowerCase;
    JS.transferKeyToUpperCase = transferKeyToUpperCase;
    JS.isStrictObject = isStrictObject;
    return JS;
}());

var commonjsGlobal = typeof globalThis !== 'undefined' ? globalThis : typeof window !== 'undefined' ? window : typeof global !== 'undefined' ? global : typeof self !== 'undefined' ? self : {};

function createCommonjsModule(fn, basedir, module) {
	return module = {
		path: basedir,
		exports: {},
		require: function (path, base) {
			return commonjsRequire(path, (base === undefined || base === null) ? module.path : base);
		}
	}, fn(module, module.exports), module.exports;
}

function getDefaultExportFromNamespaceIfNotNamed (n) {
	return n && Object.prototype.hasOwnProperty.call(n, 'default') && Object.keys(n).length === 1 ? n['default'] : n;
}

function commonjsRequire () {
	throw new Error('Dynamic requires are not currently supported by @rollup/plugin-commonjs');
}

/*! *****************************************************************************
Copyright (c) Microsoft Corporation.

Permission to use, copy, modify, and/or distribute this software for any
purpose with or without fee is hereby granted.

THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH
REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY
AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM
LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR
OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR
PERFORMANCE OF THIS SOFTWARE.
***************************************************************************** */
/* global Reflect, Promise */

var extendStatics$1 = function(d, b) {
    extendStatics$1 = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return extendStatics$1(d, b);
};

function __extends$3(d, b) {
    extendStatics$1(d, b);
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
}

var __assign$6 = function() {
    __assign$6 = Object.assign || function __assign(t) {
        for (var s, i = 1, n = arguments.length; i < n; i++) {
            s = arguments[i];
            for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p)) t[p] = s[p];
        }
        return t;
    };
    return __assign$6.apply(this, arguments);
};

function __rest(s, e) {
    var t = {};
    for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p) && e.indexOf(p) < 0)
        t[p] = s[p];
    if (s != null && typeof Object.getOwnPropertySymbols === "function")
        for (var i = 0, p = Object.getOwnPropertySymbols(s); i < p.length; i++) {
            if (e.indexOf(p[i]) < 0 && Object.prototype.propertyIsEnumerable.call(s, p[i]))
                t[p[i]] = s[p[i]];
        }
    return t;
}

function __decorate(decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
}

function __param(paramIndex, decorator) {
    return function (target, key) { decorator(target, key, paramIndex); }
}

function __metadata(metadataKey, metadataValue) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(metadataKey, metadataValue);
}

function __awaiter$7(thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
}

function __generator$7(thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
}

function __createBinding(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}

function __exportStar(m, exports) {
    for (var p in m) if (p !== "default" && !exports.hasOwnProperty(p)) exports[p] = m[p];
}

function __values(o) {
    var s = typeof Symbol === "function" && Symbol.iterator, m = s && o[s], i = 0;
    if (m) return m.call(o);
    if (o && typeof o.length === "number") return {
        next: function () {
            if (o && i >= o.length) o = void 0;
            return { value: o && o[i++], done: !o };
        }
    };
    throw new TypeError(s ? "Object is not iterable." : "Symbol.iterator is not defined.");
}

function __read$4(o, n) {
    var m = typeof Symbol === "function" && o[Symbol.iterator];
    if (!m) return o;
    var i = m.call(o), r, ar = [], e;
    try {
        while ((n === void 0 || n-- > 0) && !(r = i.next()).done) ar.push(r.value);
    }
    catch (error) { e = { error: error }; }
    finally {
        try {
            if (r && !r.done && (m = i["return"])) m.call(i);
        }
        finally { if (e) throw e.error; }
    }
    return ar;
}

function __spread$1() {
    for (var ar = [], i = 0; i < arguments.length; i++)
        ar = ar.concat(__read$4(arguments[i]));
    return ar;
}

function __spreadArrays() {
    for (var s = 0, i = 0, il = arguments.length; i < il; i++) s += arguments[i].length;
    for (var r = Array(s), k = 0, i = 0; i < il; i++)
        for (var a = arguments[i], j = 0, jl = a.length; j < jl; j++, k++)
            r[k] = a[j];
    return r;
}
function __await(v) {
    return this instanceof __await ? (this.v = v, this) : new __await(v);
}

function __asyncGenerator(thisArg, _arguments, generator) {
    if (!Symbol.asyncIterator) throw new TypeError("Symbol.asyncIterator is not defined.");
    var g = generator.apply(thisArg, _arguments || []), i, q = [];
    return i = {}, verb("next"), verb("throw"), verb("return"), i[Symbol.asyncIterator] = function () { return this; }, i;
    function verb(n) { if (g[n]) i[n] = function (v) { return new Promise(function (a, b) { q.push([n, v, a, b]) > 1 || resume(n, v); }); }; }
    function resume(n, v) { try { step(g[n](v)); } catch (e) { settle(q[0][3], e); } }
    function step(r) { r.value instanceof __await ? Promise.resolve(r.value.v).then(fulfill, reject) : settle(q[0][2], r); }
    function fulfill(value) { resume("next", value); }
    function reject(value) { resume("throw", value); }
    function settle(f, v) { if (f(v), q.shift(), q.length) resume(q[0][0], q[0][1]); }
}

function __asyncDelegator(o) {
    var i, p;
    return i = {}, verb("next"), verb("throw", function (e) { throw e; }), verb("return"), i[Symbol.iterator] = function () { return this; }, i;
    function verb(n, f) { i[n] = o[n] ? function (v) { return (p = !p) ? { value: __await(o[n](v)), done: n === "return" } : f ? f(v) : v; } : f; }
}

function __asyncValues(o) {
    if (!Symbol.asyncIterator) throw new TypeError("Symbol.asyncIterator is not defined.");
    var m = o[Symbol.asyncIterator], i;
    return m ? m.call(o) : (o = typeof __values === "function" ? __values(o) : o[Symbol.iterator](), i = {}, verb("next"), verb("throw"), verb("return"), i[Symbol.asyncIterator] = function () { return this; }, i);
    function verb(n) { i[n] = o[n] && function (v) { return new Promise(function (resolve, reject) { v = o[n](v), settle(resolve, reject, v.done, v.value); }); }; }
    function settle(resolve, reject, d, v) { Promise.resolve(v).then(function(v) { resolve({ value: v, done: d }); }, reject); }
}

function __makeTemplateObject(cooked, raw) {
    if (Object.defineProperty) { Object.defineProperty(cooked, "raw", { value: raw }); } else { cooked.raw = raw; }
    return cooked;
}
function __importStar(mod) {
    if (mod && mod.__esModule) return mod;
    var result = {};
    if (mod != null) for (var k in mod) if (Object.hasOwnProperty.call(mod, k)) result[k] = mod[k];
    result.default = mod;
    return result;
}

function __importDefault(mod) {
    return (mod && mod.__esModule) ? mod : { default: mod };
}

function __classPrivateFieldGet(receiver, privateMap) {
    if (!privateMap.has(receiver)) {
        throw new TypeError("attempted to get private field on non-instance");
    }
    return privateMap.get(receiver);
}

function __classPrivateFieldSet(receiver, privateMap, value) {
    if (!privateMap.has(receiver)) {
        throw new TypeError("attempted to set private field on non-instance");
    }
    privateMap.set(receiver, value);
    return value;
}

var tslib_es6 = /*#__PURE__*/Object.freeze({
    __proto__: null,
    __extends: __extends$3,
    get __assign () { return __assign$6; },
    __rest: __rest,
    __decorate: __decorate,
    __param: __param,
    __metadata: __metadata,
    __awaiter: __awaiter$7,
    __generator: __generator$7,
    __createBinding: __createBinding,
    __exportStar: __exportStar,
    __values: __values,
    __read: __read$4,
    __spread: __spread$1,
    __spreadArrays: __spreadArrays,
    __await: __await,
    __asyncGenerator: __asyncGenerator,
    __asyncDelegator: __asyncDelegator,
    __asyncValues: __asyncValues,
    __makeTemplateObject: __makeTemplateObject,
    __importStar: __importStar,
    __importDefault: __importDefault,
    __classPrivateFieldGet: __classPrivateFieldGet,
    __classPrivateFieldSet: __classPrivateFieldSet
});

var SHORT_TO_HEX = {};
for (var i$2 = 0; i$2 < 256; i$2++) {
    var encodedByte = i$2.toString(16).toLowerCase();
    if (encodedByte.length === 1) {
        encodedByte = "0" + encodedByte;
    }
    SHORT_TO_HEX[i$2] = encodedByte;
}
/**
 * Converts a Uint8Array of binary data to a hexadecimal encoded string.
 *
 * @param bytes The binary data to encode
 */
function toHex$1(bytes) {
    var out = "";
    for (var i = 0; i < bytes.byteLength; i++) {
        out += SHORT_TO_HEX[bytes[i]];
    }
    return out;
}

/*! https://mths.be/punycode v1.4.1 by @mathias */


/** Highest positive signed 32-bit float value */
var maxInt = 2147483647; // aka. 0x7FFFFFFF or 2^31-1

/** Bootstring parameters */
var base = 36;
var tMin = 1;
var tMax = 26;
var skew = 38;
var damp = 700;
var initialBias = 72;
var initialN = 128; // 0x80
var delimiter = '-'; // '\x2D'
var regexNonASCII = /[^\x20-\x7E]/; // unprintable ASCII chars + non-ASCII chars
var regexSeparators = /[\x2E\u3002\uFF0E\uFF61]/g; // RFC 3490 separators

/** Error messages */
var errors = {
  'overflow': 'Overflow: input needs wider integers to process',
  'not-basic': 'Illegal input >= 0x80 (not a basic code point)',
  'invalid-input': 'Invalid input'
};

/** Convenience shortcuts */
var baseMinusTMin = base - tMin;
var floor = Math.floor;
var stringFromCharCode = String.fromCharCode;

/*--------------------------------------------------------------------------*/

/**
 * A generic error utility function.
 * @private
 * @param {String} type The error type.
 * @returns {Error} Throws a `RangeError` with the applicable error message.
 */
function error(type) {
  throw new RangeError(errors[type]);
}

/**
 * A generic `Array#map` utility function.
 * @private
 * @param {Array} array The array to iterate over.
 * @param {Function} callback The function that gets called for every array
 * item.
 * @returns {Array} A new array of values returned by the callback function.
 */
function map$1(array, fn) {
  var length = array.length;
  var result = [];
  while (length--) {
    result[length] = fn(array[length]);
  }
  return result;
}

/**
 * A simple `Array#map`-like wrapper to work with domain name strings or email
 * addresses.
 * @private
 * @param {String} domain The domain name or email address.
 * @param {Function} callback The function that gets called for every
 * character.
 * @returns {Array} A new string of characters returned by the callback
 * function.
 */
function mapDomain(string, fn) {
  var parts = string.split('@');
  var result = '';
  if (parts.length > 1) {
    // In email addresses, only the domain name should be punycoded. Leave
    // the local part (i.e. everything up to `@`) intact.
    result = parts[0] + '@';
    string = parts[1];
  }
  // Avoid `split(regex)` for IE8 compatibility. See #17.
  string = string.replace(regexSeparators, '\x2E');
  var labels = string.split('.');
  var encoded = map$1(labels, fn).join('.');
  return result + encoded;
}

/**
 * Creates an array containing the numeric code points of each Unicode
 * character in the string. While JavaScript uses UCS-2 internally,
 * this function will convert a pair of surrogate halves (each of which
 * UCS-2 exposes as separate characters) into a single code point,
 * matching UTF-16.
 * @see `punycode.ucs2.encode`
 * @see <https://mathiasbynens.be/notes/javascript-encoding>
 * @memberOf punycode.ucs2
 * @name decode
 * @param {String} string The Unicode input string (UCS-2).
 * @returns {Array} The new array of code points.
 */
function ucs2decode(string) {
  var output = [],
    counter = 0,
    length = string.length,
    value,
    extra;
  while (counter < length) {
    value = string.charCodeAt(counter++);
    if (value >= 0xD800 && value <= 0xDBFF && counter < length) {
      // high surrogate, and there is a next character
      extra = string.charCodeAt(counter++);
      if ((extra & 0xFC00) == 0xDC00) { // low surrogate
        output.push(((value & 0x3FF) << 10) + (extra & 0x3FF) + 0x10000);
      } else {
        // unmatched surrogate; only append this code unit, in case the next
        // code unit is the high surrogate of a surrogate pair
        output.push(value);
        counter--;
      }
    } else {
      output.push(value);
    }
  }
  return output;
}

/**
 * Converts a digit/integer into a basic code point.
 * @see `basicToDigit()`
 * @private
 * @param {Number} digit The numeric value of a basic code point.
 * @returns {Number} The basic code point whose value (when used for
 * representing integers) is `digit`, which needs to be in the range
 * `0` to `base - 1`. If `flag` is non-zero, the uppercase form is
 * used; else, the lowercase form is used. The behavior is undefined
 * if `flag` is non-zero and `digit` has no uppercase form.
 */
function digitToBasic(digit, flag) {
  //  0..25 map to ASCII a..z or A..Z
  // 26..35 map to ASCII 0..9
  return digit + 22 + 75 * (digit < 26) - ((flag != 0) << 5);
}

/**
 * Bias adaptation function as per section 3.4 of RFC 3492.
 * https://tools.ietf.org/html/rfc3492#section-3.4
 * @private
 */
function adapt(delta, numPoints, firstTime) {
  var k = 0;
  delta = firstTime ? floor(delta / damp) : delta >> 1;
  delta += floor(delta / numPoints);
  for ( /* no initialization */ ; delta > baseMinusTMin * tMax >> 1; k += base) {
    delta = floor(delta / baseMinusTMin);
  }
  return floor(k + (baseMinusTMin + 1) * delta / (delta + skew));
}

/**
 * Converts a string of Unicode symbols (e.g. a domain name label) to a
 * Punycode string of ASCII-only symbols.
 * @memberOf punycode
 * @param {String} input The string of Unicode symbols.
 * @returns {String} The resulting Punycode string of ASCII-only symbols.
 */
function encode$1(input) {
  var n,
    delta,
    handledCPCount,
    basicLength,
    bias,
    j,
    m,
    q,
    k,
    t,
    currentValue,
    output = [],
    /** `inputLength` will hold the number of code points in `input`. */
    inputLength,
    /** Cached calculation results */
    handledCPCountPlusOne,
    baseMinusT,
    qMinusT;

  // Convert the input in UCS-2 to Unicode
  input = ucs2decode(input);

  // Cache the length
  inputLength = input.length;

  // Initialize the state
  n = initialN;
  delta = 0;
  bias = initialBias;

  // Handle the basic code points
  for (j = 0; j < inputLength; ++j) {
    currentValue = input[j];
    if (currentValue < 0x80) {
      output.push(stringFromCharCode(currentValue));
    }
  }

  handledCPCount = basicLength = output.length;

  // `handledCPCount` is the number of code points that have been handled;
  // `basicLength` is the number of basic code points.

  // Finish the basic string - if it is not empty - with a delimiter
  if (basicLength) {
    output.push(delimiter);
  }

  // Main encoding loop:
  while (handledCPCount < inputLength) {

    // All non-basic code points < n have been handled already. Find the next
    // larger one:
    for (m = maxInt, j = 0; j < inputLength; ++j) {
      currentValue = input[j];
      if (currentValue >= n && currentValue < m) {
        m = currentValue;
      }
    }

    // Increase `delta` enough to advance the decoder's <n,i> state to <m,0>,
    // but guard against overflow
    handledCPCountPlusOne = handledCPCount + 1;
    if (m - n > floor((maxInt - delta) / handledCPCountPlusOne)) {
      error('overflow');
    }

    delta += (m - n) * handledCPCountPlusOne;
    n = m;

    for (j = 0; j < inputLength; ++j) {
      currentValue = input[j];

      if (currentValue < n && ++delta > maxInt) {
        error('overflow');
      }

      if (currentValue == n) {
        // Represent delta as a generalized variable-length integer
        for (q = delta, k = base; /* no condition */ ; k += base) {
          t = k <= bias ? tMin : (k >= bias + tMax ? tMax : k - bias);
          if (q < t) {
            break;
          }
          qMinusT = q - t;
          baseMinusT = base - t;
          output.push(
            stringFromCharCode(digitToBasic(t + qMinusT % baseMinusT, 0))
          );
          q = floor(qMinusT / baseMinusT);
        }

        output.push(stringFromCharCode(digitToBasic(q, 0)));
        bias = adapt(delta, handledCPCountPlusOne, handledCPCount == basicLength);
        delta = 0;
        ++handledCPCount;
      }
    }

    ++delta;
    ++n;

  }
  return output.join('');
}

/**
 * Converts a Unicode string representing a domain name or an email address to
 * Punycode. Only the non-ASCII parts of the domain name will be converted,
 * i.e. it doesn't matter if you call it with a domain that's already in
 * ASCII.
 * @memberOf punycode
 * @param {String} input The domain name or email address to convert, as a
 * Unicode string.
 * @returns {String} The Punycode representation of the given domain name or
 * email address.
 */
function toASCII(input) {
  return mapDomain(input, function(string) {
    return regexNonASCII.test(string) ?
      'xn--' + encode$1(string) :
      string;
  });
}

var global$1 = (typeof global !== "undefined" ? global :
  typeof self !== "undefined" ? self :
  typeof window !== "undefined" ? window : {});

var lookup = [];
var revLookup = [];
var Arr = typeof Uint8Array !== 'undefined' ? Uint8Array : Array;
var inited = false;
function init () {
  inited = true;
  var code = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/';
  for (var i = 0, len = code.length; i < len; ++i) {
    lookup[i] = code[i];
    revLookup[code.charCodeAt(i)] = i;
  }

  revLookup['-'.charCodeAt(0)] = 62;
  revLookup['_'.charCodeAt(0)] = 63;
}

function toByteArray (b64) {
  if (!inited) {
    init();
  }
  var i, j, l, tmp, placeHolders, arr;
  var len = b64.length;

  if (len % 4 > 0) {
    throw new Error('Invalid string. Length must be a multiple of 4')
  }

  // the number of equal signs (place holders)
  // if there are two placeholders, than the two characters before it
  // represent one byte
  // if there is only one, then the three characters before it represent 2 bytes
  // this is just a cheap hack to not do indexOf twice
  placeHolders = b64[len - 2] === '=' ? 2 : b64[len - 1] === '=' ? 1 : 0;

  // base64 is 4/3 + up to two characters of the original data
  arr = new Arr(len * 3 / 4 - placeHolders);

  // if there are placeholders, only get up to the last complete 4 chars
  l = placeHolders > 0 ? len - 4 : len;

  var L = 0;

  for (i = 0, j = 0; i < l; i += 4, j += 3) {
    tmp = (revLookup[b64.charCodeAt(i)] << 18) | (revLookup[b64.charCodeAt(i + 1)] << 12) | (revLookup[b64.charCodeAt(i + 2)] << 6) | revLookup[b64.charCodeAt(i + 3)];
    arr[L++] = (tmp >> 16) & 0xFF;
    arr[L++] = (tmp >> 8) & 0xFF;
    arr[L++] = tmp & 0xFF;
  }

  if (placeHolders === 2) {
    tmp = (revLookup[b64.charCodeAt(i)] << 2) | (revLookup[b64.charCodeAt(i + 1)] >> 4);
    arr[L++] = tmp & 0xFF;
  } else if (placeHolders === 1) {
    tmp = (revLookup[b64.charCodeAt(i)] << 10) | (revLookup[b64.charCodeAt(i + 1)] << 4) | (revLookup[b64.charCodeAt(i + 2)] >> 2);
    arr[L++] = (tmp >> 8) & 0xFF;
    arr[L++] = tmp & 0xFF;
  }

  return arr
}

function tripletToBase64 (num) {
  return lookup[num >> 18 & 0x3F] + lookup[num >> 12 & 0x3F] + lookup[num >> 6 & 0x3F] + lookup[num & 0x3F]
}

function encodeChunk (uint8, start, end) {
  var tmp;
  var output = [];
  for (var i = start; i < end; i += 3) {
    tmp = (uint8[i] << 16) + (uint8[i + 1] << 8) + (uint8[i + 2]);
    output.push(tripletToBase64(tmp));
  }
  return output.join('')
}

function fromByteArray (uint8) {
  if (!inited) {
    init();
  }
  var tmp;
  var len = uint8.length;
  var extraBytes = len % 3; // if we have 1 byte left, pad 2 bytes
  var output = '';
  var parts = [];
  var maxChunkLength = 16383; // must be multiple of 3

  // go through the array every three bytes, we'll deal with trailing stuff later
  for (var i = 0, len2 = len - extraBytes; i < len2; i += maxChunkLength) {
    parts.push(encodeChunk(uint8, i, (i + maxChunkLength) > len2 ? len2 : (i + maxChunkLength)));
  }

  // pad the end with zeros, but make sure to not forget the extra bytes
  if (extraBytes === 1) {
    tmp = uint8[len - 1];
    output += lookup[tmp >> 2];
    output += lookup[(tmp << 4) & 0x3F];
    output += '==';
  } else if (extraBytes === 2) {
    tmp = (uint8[len - 2] << 8) + (uint8[len - 1]);
    output += lookup[tmp >> 10];
    output += lookup[(tmp >> 4) & 0x3F];
    output += lookup[(tmp << 2) & 0x3F];
    output += '=';
  }

  parts.push(output);

  return parts.join('')
}

function read (buffer, offset, isLE, mLen, nBytes) {
  var e, m;
  var eLen = nBytes * 8 - mLen - 1;
  var eMax = (1 << eLen) - 1;
  var eBias = eMax >> 1;
  var nBits = -7;
  var i = isLE ? (nBytes - 1) : 0;
  var d = isLE ? -1 : 1;
  var s = buffer[offset + i];

  i += d;

  e = s & ((1 << (-nBits)) - 1);
  s >>= (-nBits);
  nBits += eLen;
  for (; nBits > 0; e = e * 256 + buffer[offset + i], i += d, nBits -= 8) {}

  m = e & ((1 << (-nBits)) - 1);
  e >>= (-nBits);
  nBits += mLen;
  for (; nBits > 0; m = m * 256 + buffer[offset + i], i += d, nBits -= 8) {}

  if (e === 0) {
    e = 1 - eBias;
  } else if (e === eMax) {
    return m ? NaN : ((s ? -1 : 1) * Infinity)
  } else {
    m = m + Math.pow(2, mLen);
    e = e - eBias;
  }
  return (s ? -1 : 1) * m * Math.pow(2, e - mLen)
}

function write (buffer, value, offset, isLE, mLen, nBytes) {
  var e, m, c;
  var eLen = nBytes * 8 - mLen - 1;
  var eMax = (1 << eLen) - 1;
  var eBias = eMax >> 1;
  var rt = (mLen === 23 ? Math.pow(2, -24) - Math.pow(2, -77) : 0);
  var i = isLE ? 0 : (nBytes - 1);
  var d = isLE ? 1 : -1;
  var s = value < 0 || (value === 0 && 1 / value < 0) ? 1 : 0;

  value = Math.abs(value);

  if (isNaN(value) || value === Infinity) {
    m = isNaN(value) ? 1 : 0;
    e = eMax;
  } else {
    e = Math.floor(Math.log(value) / Math.LN2);
    if (value * (c = Math.pow(2, -e)) < 1) {
      e--;
      c *= 2;
    }
    if (e + eBias >= 1) {
      value += rt / c;
    } else {
      value += rt * Math.pow(2, 1 - eBias);
    }
    if (value * c >= 2) {
      e++;
      c /= 2;
    }

    if (e + eBias >= eMax) {
      m = 0;
      e = eMax;
    } else if (e + eBias >= 1) {
      m = (value * c - 1) * Math.pow(2, mLen);
      e = e + eBias;
    } else {
      m = value * Math.pow(2, eBias - 1) * Math.pow(2, mLen);
      e = 0;
    }
  }

  for (; mLen >= 8; buffer[offset + i] = m & 0xff, i += d, m /= 256, mLen -= 8) {}

  e = (e << mLen) | m;
  eLen += mLen;
  for (; eLen > 0; buffer[offset + i] = e & 0xff, i += d, e /= 256, eLen -= 8) {}

  buffer[offset + i - d] |= s * 128;
}

var toString = {}.toString;

var isArray$1 = Array.isArray || function (arr) {
  return toString.call(arr) == '[object Array]';
};

/*!
 * The buffer module from node.js, for the browser.
 *
 * @author   Feross Aboukhadijeh <feross@feross.org> <http://feross.org>
 * @license  MIT
 */

var INSPECT_MAX_BYTES = 50;

/**
 * If `Buffer.TYPED_ARRAY_SUPPORT`:
 *   === true    Use Uint8Array implementation (fastest)
 *   === false   Use Object implementation (most compatible, even IE6)
 *
 * Browsers that support typed arrays are IE 10+, Firefox 4+, Chrome 7+, Safari 5.1+,
 * Opera 11.6+, iOS 4.2+.
 *
 * Due to various browser bugs, sometimes the Object implementation will be used even
 * when the browser supports typed arrays.
 *
 * Note:
 *
 *   - Firefox 4-29 lacks support for adding new properties to `Uint8Array` instances,
 *     See: https://bugzilla.mozilla.org/show_bug.cgi?id=695438.
 *
 *   - Chrome 9-10 is missing the `TypedArray.prototype.subarray` function.
 *
 *   - IE10 has a broken `TypedArray.prototype.subarray` function which returns arrays of
 *     incorrect length in some situations.

 * We detect these buggy browsers and set `Buffer.TYPED_ARRAY_SUPPORT` to `false` so they
 * get the Object implementation, which is slower but behaves correctly.
 */
Buffer.TYPED_ARRAY_SUPPORT = global$1.TYPED_ARRAY_SUPPORT !== undefined
  ? global$1.TYPED_ARRAY_SUPPORT
  : true;

/*
 * Export kMaxLength after typed array support is determined.
 */
kMaxLength();

function kMaxLength () {
  return Buffer.TYPED_ARRAY_SUPPORT
    ? 0x7fffffff
    : 0x3fffffff
}

function createBuffer (that, length) {
  if (kMaxLength() < length) {
    throw new RangeError('Invalid typed array length')
  }
  if (Buffer.TYPED_ARRAY_SUPPORT) {
    // Return an augmented `Uint8Array` instance, for best performance
    that = new Uint8Array(length);
    that.__proto__ = Buffer.prototype;
  } else {
    // Fallback: Return an object instance of the Buffer class
    if (that === null) {
      that = new Buffer(length);
    }
    that.length = length;
  }

  return that
}

/**
 * The Buffer constructor returns instances of `Uint8Array` that have their
 * prototype changed to `Buffer.prototype`. Furthermore, `Buffer` is a subclass of
 * `Uint8Array`, so the returned instances will have all the node `Buffer` methods
 * and the `Uint8Array` methods. Square bracket notation works as expected -- it
 * returns a single octet.
 *
 * The `Uint8Array` prototype remains unmodified.
 */

function Buffer (arg, encodingOrOffset, length) {
  if (!Buffer.TYPED_ARRAY_SUPPORT && !(this instanceof Buffer)) {
    return new Buffer(arg, encodingOrOffset, length)
  }

  // Common case.
  if (typeof arg === 'number') {
    if (typeof encodingOrOffset === 'string') {
      throw new Error(
        'If encoding is specified then the first argument must be a string'
      )
    }
    return allocUnsafe(this, arg)
  }
  return from(this, arg, encodingOrOffset, length)
}

Buffer.poolSize = 8192; // not used by this implementation

// TODO: Legacy, not needed anymore. Remove in next major version.
Buffer._augment = function (arr) {
  arr.__proto__ = Buffer.prototype;
  return arr
};

function from (that, value, encodingOrOffset, length) {
  if (typeof value === 'number') {
    throw new TypeError('"value" argument must not be a number')
  }

  if (typeof ArrayBuffer !== 'undefined' && value instanceof ArrayBuffer) {
    return fromArrayBuffer(that, value, encodingOrOffset, length)
  }

  if (typeof value === 'string') {
    return fromString(that, value, encodingOrOffset)
  }

  return fromObject(that, value)
}

/**
 * Functionally equivalent to Buffer(arg, encoding) but throws a TypeError
 * if value is a number.
 * Buffer.from(str[, encoding])
 * Buffer.from(array)
 * Buffer.from(buffer)
 * Buffer.from(arrayBuffer[, byteOffset[, length]])
 **/
Buffer.from = function (value, encodingOrOffset, length) {
  return from(null, value, encodingOrOffset, length)
};

if (Buffer.TYPED_ARRAY_SUPPORT) {
  Buffer.prototype.__proto__ = Uint8Array.prototype;
  Buffer.__proto__ = Uint8Array;
}

function assertSize (size) {
  if (typeof size !== 'number') {
    throw new TypeError('"size" argument must be a number')
  } else if (size < 0) {
    throw new RangeError('"size" argument must not be negative')
  }
}

function alloc (that, size, fill, encoding) {
  assertSize(size);
  if (size <= 0) {
    return createBuffer(that, size)
  }
  if (fill !== undefined) {
    // Only pay attention to encoding if it's a string. This
    // prevents accidentally sending in a number that would
    // be interpretted as a start offset.
    return typeof encoding === 'string'
      ? createBuffer(that, size).fill(fill, encoding)
      : createBuffer(that, size).fill(fill)
  }
  return createBuffer(that, size)
}

/**
 * Creates a new filled Buffer instance.
 * alloc(size[, fill[, encoding]])
 **/
Buffer.alloc = function (size, fill, encoding) {
  return alloc(null, size, fill, encoding)
};

function allocUnsafe (that, size) {
  assertSize(size);
  that = createBuffer(that, size < 0 ? 0 : checked(size) | 0);
  if (!Buffer.TYPED_ARRAY_SUPPORT) {
    for (var i = 0; i < size; ++i) {
      that[i] = 0;
    }
  }
  return that
}

/**
 * Equivalent to Buffer(num), by default creates a non-zero-filled Buffer instance.
 * */
Buffer.allocUnsafe = function (size) {
  return allocUnsafe(null, size)
};
/**
 * Equivalent to SlowBuffer(num), by default creates a non-zero-filled Buffer instance.
 */
Buffer.allocUnsafeSlow = function (size) {
  return allocUnsafe(null, size)
};

function fromString (that, string, encoding) {
  if (typeof encoding !== 'string' || encoding === '') {
    encoding = 'utf8';
  }

  if (!Buffer.isEncoding(encoding)) {
    throw new TypeError('"encoding" must be a valid string encoding')
  }

  var length = byteLength(string, encoding) | 0;
  that = createBuffer(that, length);

  var actual = that.write(string, encoding);

  if (actual !== length) {
    // Writing a hex string, for example, that contains invalid characters will
    // cause everything after the first invalid character to be ignored. (e.g.
    // 'abxxcd' will be treated as 'ab')
    that = that.slice(0, actual);
  }

  return that
}

function fromArrayLike (that, array) {
  var length = array.length < 0 ? 0 : checked(array.length) | 0;
  that = createBuffer(that, length);
  for (var i = 0; i < length; i += 1) {
    that[i] = array[i] & 255;
  }
  return that
}

function fromArrayBuffer (that, array, byteOffset, length) {
  array.byteLength; // this throws if `array` is not a valid ArrayBuffer

  if (byteOffset < 0 || array.byteLength < byteOffset) {
    throw new RangeError('\'offset\' is out of bounds')
  }

  if (array.byteLength < byteOffset + (length || 0)) {
    throw new RangeError('\'length\' is out of bounds')
  }

  if (byteOffset === undefined && length === undefined) {
    array = new Uint8Array(array);
  } else if (length === undefined) {
    array = new Uint8Array(array, byteOffset);
  } else {
    array = new Uint8Array(array, byteOffset, length);
  }

  if (Buffer.TYPED_ARRAY_SUPPORT) {
    // Return an augmented `Uint8Array` instance, for best performance
    that = array;
    that.__proto__ = Buffer.prototype;
  } else {
    // Fallback: Return an object instance of the Buffer class
    that = fromArrayLike(that, array);
  }
  return that
}

function fromObject (that, obj) {
  if (internalIsBuffer(obj)) {
    var len = checked(obj.length) | 0;
    that = createBuffer(that, len);

    if (that.length === 0) {
      return that
    }

    obj.copy(that, 0, 0, len);
    return that
  }

  if (obj) {
    if ((typeof ArrayBuffer !== 'undefined' &&
        obj.buffer instanceof ArrayBuffer) || 'length' in obj) {
      if (typeof obj.length !== 'number' || isnan(obj.length)) {
        return createBuffer(that, 0)
      }
      return fromArrayLike(that, obj)
    }

    if (obj.type === 'Buffer' && isArray$1(obj.data)) {
      return fromArrayLike(that, obj.data)
    }
  }

  throw new TypeError('First argument must be a string, Buffer, ArrayBuffer, Array, or array-like object.')
}

function checked (length) {
  // Note: cannot use `length < kMaxLength()` here because that fails when
  // length is NaN (which is otherwise coerced to zero.)
  if (length >= kMaxLength()) {
    throw new RangeError('Attempt to allocate Buffer larger than maximum ' +
                         'size: 0x' + kMaxLength().toString(16) + ' bytes')
  }
  return length | 0
}
Buffer.isBuffer = isBuffer;
function internalIsBuffer (b) {
  return !!(b != null && b._isBuffer)
}

Buffer.compare = function compare (a, b) {
  if (!internalIsBuffer(a) || !internalIsBuffer(b)) {
    throw new TypeError('Arguments must be Buffers')
  }

  if (a === b) return 0

  var x = a.length;
  var y = b.length;

  for (var i = 0, len = Math.min(x, y); i < len; ++i) {
    if (a[i] !== b[i]) {
      x = a[i];
      y = b[i];
      break
    }
  }

  if (x < y) return -1
  if (y < x) return 1
  return 0
};

Buffer.isEncoding = function isEncoding (encoding) {
  switch (String(encoding).toLowerCase()) {
    case 'hex':
    case 'utf8':
    case 'utf-8':
    case 'ascii':
    case 'latin1':
    case 'binary':
    case 'base64':
    case 'ucs2':
    case 'ucs-2':
    case 'utf16le':
    case 'utf-16le':
      return true
    default:
      return false
  }
};

Buffer.concat = function concat (list, length) {
  if (!isArray$1(list)) {
    throw new TypeError('"list" argument must be an Array of Buffers')
  }

  if (list.length === 0) {
    return Buffer.alloc(0)
  }

  var i;
  if (length === undefined) {
    length = 0;
    for (i = 0; i < list.length; ++i) {
      length += list[i].length;
    }
  }

  var buffer = Buffer.allocUnsafe(length);
  var pos = 0;
  for (i = 0; i < list.length; ++i) {
    var buf = list[i];
    if (!internalIsBuffer(buf)) {
      throw new TypeError('"list" argument must be an Array of Buffers')
    }
    buf.copy(buffer, pos);
    pos += buf.length;
  }
  return buffer
};

function byteLength (string, encoding) {
  if (internalIsBuffer(string)) {
    return string.length
  }
  if (typeof ArrayBuffer !== 'undefined' && typeof ArrayBuffer.isView === 'function' &&
      (ArrayBuffer.isView(string) || string instanceof ArrayBuffer)) {
    return string.byteLength
  }
  if (typeof string !== 'string') {
    string = '' + string;
  }

  var len = string.length;
  if (len === 0) return 0

  // Use a for loop to avoid recursion
  var loweredCase = false;
  for (;;) {
    switch (encoding) {
      case 'ascii':
      case 'latin1':
      case 'binary':
        return len
      case 'utf8':
      case 'utf-8':
      case undefined:
        return utf8ToBytes(string).length
      case 'ucs2':
      case 'ucs-2':
      case 'utf16le':
      case 'utf-16le':
        return len * 2
      case 'hex':
        return len >>> 1
      case 'base64':
        return base64ToBytes(string).length
      default:
        if (loweredCase) return utf8ToBytes(string).length // assume utf8
        encoding = ('' + encoding).toLowerCase();
        loweredCase = true;
    }
  }
}
Buffer.byteLength = byteLength;

function slowToString (encoding, start, end) {
  var loweredCase = false;

  // No need to verify that "this.length <= MAX_UINT32" since it's a read-only
  // property of a typed array.

  // This behaves neither like String nor Uint8Array in that we set start/end
  // to their upper/lower bounds if the value passed is out of range.
  // undefined is handled specially as per ECMA-262 6th Edition,
  // Section 13.3.3.7 Runtime Semantics: KeyedBindingInitialization.
  if (start === undefined || start < 0) {
    start = 0;
  }
  // Return early if start > this.length. Done here to prevent potential uint32
  // coercion fail below.
  if (start > this.length) {
    return ''
  }

  if (end === undefined || end > this.length) {
    end = this.length;
  }

  if (end <= 0) {
    return ''
  }

  // Force coersion to uint32. This will also coerce falsey/NaN values to 0.
  end >>>= 0;
  start >>>= 0;

  if (end <= start) {
    return ''
  }

  if (!encoding) encoding = 'utf8';

  while (true) {
    switch (encoding) {
      case 'hex':
        return hexSlice(this, start, end)

      case 'utf8':
      case 'utf-8':
        return utf8Slice(this, start, end)

      case 'ascii':
        return asciiSlice(this, start, end)

      case 'latin1':
      case 'binary':
        return latin1Slice(this, start, end)

      case 'base64':
        return base64Slice(this, start, end)

      case 'ucs2':
      case 'ucs-2':
      case 'utf16le':
      case 'utf-16le':
        return utf16leSlice(this, start, end)

      default:
        if (loweredCase) throw new TypeError('Unknown encoding: ' + encoding)
        encoding = (encoding + '').toLowerCase();
        loweredCase = true;
    }
  }
}

// The property is used by `Buffer.isBuffer` and `is-buffer` (in Safari 5-7) to detect
// Buffer instances.
Buffer.prototype._isBuffer = true;

function swap (b, n, m) {
  var i = b[n];
  b[n] = b[m];
  b[m] = i;
}

Buffer.prototype.swap16 = function swap16 () {
  var len = this.length;
  if (len % 2 !== 0) {
    throw new RangeError('Buffer size must be a multiple of 16-bits')
  }
  for (var i = 0; i < len; i += 2) {
    swap(this, i, i + 1);
  }
  return this
};

Buffer.prototype.swap32 = function swap32 () {
  var len = this.length;
  if (len % 4 !== 0) {
    throw new RangeError('Buffer size must be a multiple of 32-bits')
  }
  for (var i = 0; i < len; i += 4) {
    swap(this, i, i + 3);
    swap(this, i + 1, i + 2);
  }
  return this
};

Buffer.prototype.swap64 = function swap64 () {
  var len = this.length;
  if (len % 8 !== 0) {
    throw new RangeError('Buffer size must be a multiple of 64-bits')
  }
  for (var i = 0; i < len; i += 8) {
    swap(this, i, i + 7);
    swap(this, i + 1, i + 6);
    swap(this, i + 2, i + 5);
    swap(this, i + 3, i + 4);
  }
  return this
};

Buffer.prototype.toString = function toString () {
  var length = this.length | 0;
  if (length === 0) return ''
  if (arguments.length === 0) return utf8Slice(this, 0, length)
  return slowToString.apply(this, arguments)
};

Buffer.prototype.equals = function equals (b) {
  if (!internalIsBuffer(b)) throw new TypeError('Argument must be a Buffer')
  if (this === b) return true
  return Buffer.compare(this, b) === 0
};

Buffer.prototype.inspect = function inspect () {
  var str = '';
  var max = INSPECT_MAX_BYTES;
  if (this.length > 0) {
    str = this.toString('hex', 0, max).match(/.{2}/g).join(' ');
    if (this.length > max) str += ' ... ';
  }
  return '<Buffer ' + str + '>'
};

Buffer.prototype.compare = function compare (target, start, end, thisStart, thisEnd) {
  if (!internalIsBuffer(target)) {
    throw new TypeError('Argument must be a Buffer')
  }

  if (start === undefined) {
    start = 0;
  }
  if (end === undefined) {
    end = target ? target.length : 0;
  }
  if (thisStart === undefined) {
    thisStart = 0;
  }
  if (thisEnd === undefined) {
    thisEnd = this.length;
  }

  if (start < 0 || end > target.length || thisStart < 0 || thisEnd > this.length) {
    throw new RangeError('out of range index')
  }

  if (thisStart >= thisEnd && start >= end) {
    return 0
  }
  if (thisStart >= thisEnd) {
    return -1
  }
  if (start >= end) {
    return 1
  }

  start >>>= 0;
  end >>>= 0;
  thisStart >>>= 0;
  thisEnd >>>= 0;

  if (this === target) return 0

  var x = thisEnd - thisStart;
  var y = end - start;
  var len = Math.min(x, y);

  var thisCopy = this.slice(thisStart, thisEnd);
  var targetCopy = target.slice(start, end);

  for (var i = 0; i < len; ++i) {
    if (thisCopy[i] !== targetCopy[i]) {
      x = thisCopy[i];
      y = targetCopy[i];
      break
    }
  }

  if (x < y) return -1
  if (y < x) return 1
  return 0
};

// Finds either the first index of `val` in `buffer` at offset >= `byteOffset`,
// OR the last index of `val` in `buffer` at offset <= `byteOffset`.
//
// Arguments:
// - buffer - a Buffer to search
// - val - a string, Buffer, or number
// - byteOffset - an index into `buffer`; will be clamped to an int32
// - encoding - an optional encoding, relevant is val is a string
// - dir - true for indexOf, false for lastIndexOf
function bidirectionalIndexOf (buffer, val, byteOffset, encoding, dir) {
  // Empty buffer means no match
  if (buffer.length === 0) return -1

  // Normalize byteOffset
  if (typeof byteOffset === 'string') {
    encoding = byteOffset;
    byteOffset = 0;
  } else if (byteOffset > 0x7fffffff) {
    byteOffset = 0x7fffffff;
  } else if (byteOffset < -0x80000000) {
    byteOffset = -0x80000000;
  }
  byteOffset = +byteOffset;  // Coerce to Number.
  if (isNaN(byteOffset)) {
    // byteOffset: it it's undefined, null, NaN, "foo", etc, search whole buffer
    byteOffset = dir ? 0 : (buffer.length - 1);
  }

  // Normalize byteOffset: negative offsets start from the end of the buffer
  if (byteOffset < 0) byteOffset = buffer.length + byteOffset;
  if (byteOffset >= buffer.length) {
    if (dir) return -1
    else byteOffset = buffer.length - 1;
  } else if (byteOffset < 0) {
    if (dir) byteOffset = 0;
    else return -1
  }

  // Normalize val
  if (typeof val === 'string') {
    val = Buffer.from(val, encoding);
  }

  // Finally, search either indexOf (if dir is true) or lastIndexOf
  if (internalIsBuffer(val)) {
    // Special case: looking for empty string/buffer always fails
    if (val.length === 0) {
      return -1
    }
    return arrayIndexOf(buffer, val, byteOffset, encoding, dir)
  } else if (typeof val === 'number') {
    val = val & 0xFF; // Search for a byte value [0-255]
    if (Buffer.TYPED_ARRAY_SUPPORT &&
        typeof Uint8Array.prototype.indexOf === 'function') {
      if (dir) {
        return Uint8Array.prototype.indexOf.call(buffer, val, byteOffset)
      } else {
        return Uint8Array.prototype.lastIndexOf.call(buffer, val, byteOffset)
      }
    }
    return arrayIndexOf(buffer, [ val ], byteOffset, encoding, dir)
  }

  throw new TypeError('val must be string, number or Buffer')
}

function arrayIndexOf (arr, val, byteOffset, encoding, dir) {
  var indexSize = 1;
  var arrLength = arr.length;
  var valLength = val.length;

  if (encoding !== undefined) {
    encoding = String(encoding).toLowerCase();
    if (encoding === 'ucs2' || encoding === 'ucs-2' ||
        encoding === 'utf16le' || encoding === 'utf-16le') {
      if (arr.length < 2 || val.length < 2) {
        return -1
      }
      indexSize = 2;
      arrLength /= 2;
      valLength /= 2;
      byteOffset /= 2;
    }
  }

  function read (buf, i) {
    if (indexSize === 1) {
      return buf[i]
    } else {
      return buf.readUInt16BE(i * indexSize)
    }
  }

  var i;
  if (dir) {
    var foundIndex = -1;
    for (i = byteOffset; i < arrLength; i++) {
      if (read(arr, i) === read(val, foundIndex === -1 ? 0 : i - foundIndex)) {
        if (foundIndex === -1) foundIndex = i;
        if (i - foundIndex + 1 === valLength) return foundIndex * indexSize
      } else {
        if (foundIndex !== -1) i -= i - foundIndex;
        foundIndex = -1;
      }
    }
  } else {
    if (byteOffset + valLength > arrLength) byteOffset = arrLength - valLength;
    for (i = byteOffset; i >= 0; i--) {
      var found = true;
      for (var j = 0; j < valLength; j++) {
        if (read(arr, i + j) !== read(val, j)) {
          found = false;
          break
        }
      }
      if (found) return i
    }
  }

  return -1
}

Buffer.prototype.includes = function includes (val, byteOffset, encoding) {
  return this.indexOf(val, byteOffset, encoding) !== -1
};

Buffer.prototype.indexOf = function indexOf (val, byteOffset, encoding) {
  return bidirectionalIndexOf(this, val, byteOffset, encoding, true)
};

Buffer.prototype.lastIndexOf = function lastIndexOf (val, byteOffset, encoding) {
  return bidirectionalIndexOf(this, val, byteOffset, encoding, false)
};

function hexWrite (buf, string, offset, length) {
  offset = Number(offset) || 0;
  var remaining = buf.length - offset;
  if (!length) {
    length = remaining;
  } else {
    length = Number(length);
    if (length > remaining) {
      length = remaining;
    }
  }

  // must be an even number of digits
  var strLen = string.length;
  if (strLen % 2 !== 0) throw new TypeError('Invalid hex string')

  if (length > strLen / 2) {
    length = strLen / 2;
  }
  for (var i = 0; i < length; ++i) {
    var parsed = parseInt(string.substr(i * 2, 2), 16);
    if (isNaN(parsed)) return i
    buf[offset + i] = parsed;
  }
  return i
}

function utf8Write (buf, string, offset, length) {
  return blitBuffer(utf8ToBytes(string, buf.length - offset), buf, offset, length)
}

function asciiWrite (buf, string, offset, length) {
  return blitBuffer(asciiToBytes(string), buf, offset, length)
}

function latin1Write (buf, string, offset, length) {
  return asciiWrite(buf, string, offset, length)
}

function base64Write (buf, string, offset, length) {
  return blitBuffer(base64ToBytes(string), buf, offset, length)
}

function ucs2Write (buf, string, offset, length) {
  return blitBuffer(utf16leToBytes(string, buf.length - offset), buf, offset, length)
}

Buffer.prototype.write = function write (string, offset, length, encoding) {
  // Buffer#write(string)
  if (offset === undefined) {
    encoding = 'utf8';
    length = this.length;
    offset = 0;
  // Buffer#write(string, encoding)
  } else if (length === undefined && typeof offset === 'string') {
    encoding = offset;
    length = this.length;
    offset = 0;
  // Buffer#write(string, offset[, length][, encoding])
  } else if (isFinite(offset)) {
    offset = offset | 0;
    if (isFinite(length)) {
      length = length | 0;
      if (encoding === undefined) encoding = 'utf8';
    } else {
      encoding = length;
      length = undefined;
    }
  // legacy write(string, encoding, offset, length) - remove in v0.13
  } else {
    throw new Error(
      'Buffer.write(string, encoding, offset[, length]) is no longer supported'
    )
  }

  var remaining = this.length - offset;
  if (length === undefined || length > remaining) length = remaining;

  if ((string.length > 0 && (length < 0 || offset < 0)) || offset > this.length) {
    throw new RangeError('Attempt to write outside buffer bounds')
  }

  if (!encoding) encoding = 'utf8';

  var loweredCase = false;
  for (;;) {
    switch (encoding) {
      case 'hex':
        return hexWrite(this, string, offset, length)

      case 'utf8':
      case 'utf-8':
        return utf8Write(this, string, offset, length)

      case 'ascii':
        return asciiWrite(this, string, offset, length)

      case 'latin1':
      case 'binary':
        return latin1Write(this, string, offset, length)

      case 'base64':
        // Warning: maxLength not taken into account in base64Write
        return base64Write(this, string, offset, length)

      case 'ucs2':
      case 'ucs-2':
      case 'utf16le':
      case 'utf-16le':
        return ucs2Write(this, string, offset, length)

      default:
        if (loweredCase) throw new TypeError('Unknown encoding: ' + encoding)
        encoding = ('' + encoding).toLowerCase();
        loweredCase = true;
    }
  }
};

Buffer.prototype.toJSON = function toJSON () {
  return {
    type: 'Buffer',
    data: Array.prototype.slice.call(this._arr || this, 0)
  }
};

function base64Slice (buf, start, end) {
  if (start === 0 && end === buf.length) {
    return fromByteArray(buf)
  } else {
    return fromByteArray(buf.slice(start, end))
  }
}

function utf8Slice (buf, start, end) {
  end = Math.min(buf.length, end);
  var res = [];

  var i = start;
  while (i < end) {
    var firstByte = buf[i];
    var codePoint = null;
    var bytesPerSequence = (firstByte > 0xEF) ? 4
      : (firstByte > 0xDF) ? 3
      : (firstByte > 0xBF) ? 2
      : 1;

    if (i + bytesPerSequence <= end) {
      var secondByte, thirdByte, fourthByte, tempCodePoint;

      switch (bytesPerSequence) {
        case 1:
          if (firstByte < 0x80) {
            codePoint = firstByte;
          }
          break
        case 2:
          secondByte = buf[i + 1];
          if ((secondByte & 0xC0) === 0x80) {
            tempCodePoint = (firstByte & 0x1F) << 0x6 | (secondByte & 0x3F);
            if (tempCodePoint > 0x7F) {
              codePoint = tempCodePoint;
            }
          }
          break
        case 3:
          secondByte = buf[i + 1];
          thirdByte = buf[i + 2];
          if ((secondByte & 0xC0) === 0x80 && (thirdByte & 0xC0) === 0x80) {
            tempCodePoint = (firstByte & 0xF) << 0xC | (secondByte & 0x3F) << 0x6 | (thirdByte & 0x3F);
            if (tempCodePoint > 0x7FF && (tempCodePoint < 0xD800 || tempCodePoint > 0xDFFF)) {
              codePoint = tempCodePoint;
            }
          }
          break
        case 4:
          secondByte = buf[i + 1];
          thirdByte = buf[i + 2];
          fourthByte = buf[i + 3];
          if ((secondByte & 0xC0) === 0x80 && (thirdByte & 0xC0) === 0x80 && (fourthByte & 0xC0) === 0x80) {
            tempCodePoint = (firstByte & 0xF) << 0x12 | (secondByte & 0x3F) << 0xC | (thirdByte & 0x3F) << 0x6 | (fourthByte & 0x3F);
            if (tempCodePoint > 0xFFFF && tempCodePoint < 0x110000) {
              codePoint = tempCodePoint;
            }
          }
      }
    }

    if (codePoint === null) {
      // we did not generate a valid codePoint so insert a
      // replacement char (U+FFFD) and advance only 1 byte
      codePoint = 0xFFFD;
      bytesPerSequence = 1;
    } else if (codePoint > 0xFFFF) {
      // encode to utf16 (surrogate pair dance)
      codePoint -= 0x10000;
      res.push(codePoint >>> 10 & 0x3FF | 0xD800);
      codePoint = 0xDC00 | codePoint & 0x3FF;
    }

    res.push(codePoint);
    i += bytesPerSequence;
  }

  return decodeCodePointsArray(res)
}

// Based on http://stackoverflow.com/a/22747272/680742, the browser with
// the lowest limit is Chrome, with 0x10000 args.
// We go 1 magnitude less, for safety
var MAX_ARGUMENTS_LENGTH = 0x1000;

function decodeCodePointsArray (codePoints) {
  var len = codePoints.length;
  if (len <= MAX_ARGUMENTS_LENGTH) {
    return String.fromCharCode.apply(String, codePoints) // avoid extra slice()
  }

  // Decode in chunks to avoid "call stack size exceeded".
  var res = '';
  var i = 0;
  while (i < len) {
    res += String.fromCharCode.apply(
      String,
      codePoints.slice(i, i += MAX_ARGUMENTS_LENGTH)
    );
  }
  return res
}

function asciiSlice (buf, start, end) {
  var ret = '';
  end = Math.min(buf.length, end);

  for (var i = start; i < end; ++i) {
    ret += String.fromCharCode(buf[i] & 0x7F);
  }
  return ret
}

function latin1Slice (buf, start, end) {
  var ret = '';
  end = Math.min(buf.length, end);

  for (var i = start; i < end; ++i) {
    ret += String.fromCharCode(buf[i]);
  }
  return ret
}

function hexSlice (buf, start, end) {
  var len = buf.length;

  if (!start || start < 0) start = 0;
  if (!end || end < 0 || end > len) end = len;

  var out = '';
  for (var i = start; i < end; ++i) {
    out += toHex(buf[i]);
  }
  return out
}

function utf16leSlice (buf, start, end) {
  var bytes = buf.slice(start, end);
  var res = '';
  for (var i = 0; i < bytes.length; i += 2) {
    res += String.fromCharCode(bytes[i] + bytes[i + 1] * 256);
  }
  return res
}

Buffer.prototype.slice = function slice (start, end) {
  var len = this.length;
  start = ~~start;
  end = end === undefined ? len : ~~end;

  if (start < 0) {
    start += len;
    if (start < 0) start = 0;
  } else if (start > len) {
    start = len;
  }

  if (end < 0) {
    end += len;
    if (end < 0) end = 0;
  } else if (end > len) {
    end = len;
  }

  if (end < start) end = start;

  var newBuf;
  if (Buffer.TYPED_ARRAY_SUPPORT) {
    newBuf = this.subarray(start, end);
    newBuf.__proto__ = Buffer.prototype;
  } else {
    var sliceLen = end - start;
    newBuf = new Buffer(sliceLen, undefined);
    for (var i = 0; i < sliceLen; ++i) {
      newBuf[i] = this[i + start];
    }
  }

  return newBuf
};

/*
 * Need to make sure that buffer isn't trying to write out of bounds.
 */
function checkOffset (offset, ext, length) {
  if ((offset % 1) !== 0 || offset < 0) throw new RangeError('offset is not uint')
  if (offset + ext > length) throw new RangeError('Trying to access beyond buffer length')
}

Buffer.prototype.readUIntLE = function readUIntLE (offset, byteLength, noAssert) {
  offset = offset | 0;
  byteLength = byteLength | 0;
  if (!noAssert) checkOffset(offset, byteLength, this.length);

  var val = this[offset];
  var mul = 1;
  var i = 0;
  while (++i < byteLength && (mul *= 0x100)) {
    val += this[offset + i] * mul;
  }

  return val
};

Buffer.prototype.readUIntBE = function readUIntBE (offset, byteLength, noAssert) {
  offset = offset | 0;
  byteLength = byteLength | 0;
  if (!noAssert) {
    checkOffset(offset, byteLength, this.length);
  }

  var val = this[offset + --byteLength];
  var mul = 1;
  while (byteLength > 0 && (mul *= 0x100)) {
    val += this[offset + --byteLength] * mul;
  }

  return val
};

Buffer.prototype.readUInt8 = function readUInt8 (offset, noAssert) {
  if (!noAssert) checkOffset(offset, 1, this.length);
  return this[offset]
};

Buffer.prototype.readUInt16LE = function readUInt16LE (offset, noAssert) {
  if (!noAssert) checkOffset(offset, 2, this.length);
  return this[offset] | (this[offset + 1] << 8)
};

Buffer.prototype.readUInt16BE = function readUInt16BE (offset, noAssert) {
  if (!noAssert) checkOffset(offset, 2, this.length);
  return (this[offset] << 8) | this[offset + 1]
};

Buffer.prototype.readUInt32LE = function readUInt32LE (offset, noAssert) {
  if (!noAssert) checkOffset(offset, 4, this.length);

  return ((this[offset]) |
      (this[offset + 1] << 8) |
      (this[offset + 2] << 16)) +
      (this[offset + 3] * 0x1000000)
};

Buffer.prototype.readUInt32BE = function readUInt32BE (offset, noAssert) {
  if (!noAssert) checkOffset(offset, 4, this.length);

  return (this[offset] * 0x1000000) +
    ((this[offset + 1] << 16) |
    (this[offset + 2] << 8) |
    this[offset + 3])
};

Buffer.prototype.readIntLE = function readIntLE (offset, byteLength, noAssert) {
  offset = offset | 0;
  byteLength = byteLength | 0;
  if (!noAssert) checkOffset(offset, byteLength, this.length);

  var val = this[offset];
  var mul = 1;
  var i = 0;
  while (++i < byteLength && (mul *= 0x100)) {
    val += this[offset + i] * mul;
  }
  mul *= 0x80;

  if (val >= mul) val -= Math.pow(2, 8 * byteLength);

  return val
};

Buffer.prototype.readIntBE = function readIntBE (offset, byteLength, noAssert) {
  offset = offset | 0;
  byteLength = byteLength | 0;
  if (!noAssert) checkOffset(offset, byteLength, this.length);

  var i = byteLength;
  var mul = 1;
  var val = this[offset + --i];
  while (i > 0 && (mul *= 0x100)) {
    val += this[offset + --i] * mul;
  }
  mul *= 0x80;

  if (val >= mul) val -= Math.pow(2, 8 * byteLength);

  return val
};

Buffer.prototype.readInt8 = function readInt8 (offset, noAssert) {
  if (!noAssert) checkOffset(offset, 1, this.length);
  if (!(this[offset] & 0x80)) return (this[offset])
  return ((0xff - this[offset] + 1) * -1)
};

Buffer.prototype.readInt16LE = function readInt16LE (offset, noAssert) {
  if (!noAssert) checkOffset(offset, 2, this.length);
  var val = this[offset] | (this[offset + 1] << 8);
  return (val & 0x8000) ? val | 0xFFFF0000 : val
};

Buffer.prototype.readInt16BE = function readInt16BE (offset, noAssert) {
  if (!noAssert) checkOffset(offset, 2, this.length);
  var val = this[offset + 1] | (this[offset] << 8);
  return (val & 0x8000) ? val | 0xFFFF0000 : val
};

Buffer.prototype.readInt32LE = function readInt32LE (offset, noAssert) {
  if (!noAssert) checkOffset(offset, 4, this.length);

  return (this[offset]) |
    (this[offset + 1] << 8) |
    (this[offset + 2] << 16) |
    (this[offset + 3] << 24)
};

Buffer.prototype.readInt32BE = function readInt32BE (offset, noAssert) {
  if (!noAssert) checkOffset(offset, 4, this.length);

  return (this[offset] << 24) |
    (this[offset + 1] << 16) |
    (this[offset + 2] << 8) |
    (this[offset + 3])
};

Buffer.prototype.readFloatLE = function readFloatLE (offset, noAssert) {
  if (!noAssert) checkOffset(offset, 4, this.length);
  return read(this, offset, true, 23, 4)
};

Buffer.prototype.readFloatBE = function readFloatBE (offset, noAssert) {
  if (!noAssert) checkOffset(offset, 4, this.length);
  return read(this, offset, false, 23, 4)
};

Buffer.prototype.readDoubleLE = function readDoubleLE (offset, noAssert) {
  if (!noAssert) checkOffset(offset, 8, this.length);
  return read(this, offset, true, 52, 8)
};

Buffer.prototype.readDoubleBE = function readDoubleBE (offset, noAssert) {
  if (!noAssert) checkOffset(offset, 8, this.length);
  return read(this, offset, false, 52, 8)
};

function checkInt (buf, value, offset, ext, max, min) {
  if (!internalIsBuffer(buf)) throw new TypeError('"buffer" argument must be a Buffer instance')
  if (value > max || value < min) throw new RangeError('"value" argument is out of bounds')
  if (offset + ext > buf.length) throw new RangeError('Index out of range')
}

Buffer.prototype.writeUIntLE = function writeUIntLE (value, offset, byteLength, noAssert) {
  value = +value;
  offset = offset | 0;
  byteLength = byteLength | 0;
  if (!noAssert) {
    var maxBytes = Math.pow(2, 8 * byteLength) - 1;
    checkInt(this, value, offset, byteLength, maxBytes, 0);
  }

  var mul = 1;
  var i = 0;
  this[offset] = value & 0xFF;
  while (++i < byteLength && (mul *= 0x100)) {
    this[offset + i] = (value / mul) & 0xFF;
  }

  return offset + byteLength
};

Buffer.prototype.writeUIntBE = function writeUIntBE (value, offset, byteLength, noAssert) {
  value = +value;
  offset = offset | 0;
  byteLength = byteLength | 0;
  if (!noAssert) {
    var maxBytes = Math.pow(2, 8 * byteLength) - 1;
    checkInt(this, value, offset, byteLength, maxBytes, 0);
  }

  var i = byteLength - 1;
  var mul = 1;
  this[offset + i] = value & 0xFF;
  while (--i >= 0 && (mul *= 0x100)) {
    this[offset + i] = (value / mul) & 0xFF;
  }

  return offset + byteLength
};

Buffer.prototype.writeUInt8 = function writeUInt8 (value, offset, noAssert) {
  value = +value;
  offset = offset | 0;
  if (!noAssert) checkInt(this, value, offset, 1, 0xff, 0);
  if (!Buffer.TYPED_ARRAY_SUPPORT) value = Math.floor(value);
  this[offset] = (value & 0xff);
  return offset + 1
};

function objectWriteUInt16 (buf, value, offset, littleEndian) {
  if (value < 0) value = 0xffff + value + 1;
  for (var i = 0, j = Math.min(buf.length - offset, 2); i < j; ++i) {
    buf[offset + i] = (value & (0xff << (8 * (littleEndian ? i : 1 - i)))) >>>
      (littleEndian ? i : 1 - i) * 8;
  }
}

Buffer.prototype.writeUInt16LE = function writeUInt16LE (value, offset, noAssert) {
  value = +value;
  offset = offset | 0;
  if (!noAssert) checkInt(this, value, offset, 2, 0xffff, 0);
  if (Buffer.TYPED_ARRAY_SUPPORT) {
    this[offset] = (value & 0xff);
    this[offset + 1] = (value >>> 8);
  } else {
    objectWriteUInt16(this, value, offset, true);
  }
  return offset + 2
};

Buffer.prototype.writeUInt16BE = function writeUInt16BE (value, offset, noAssert) {
  value = +value;
  offset = offset | 0;
  if (!noAssert) checkInt(this, value, offset, 2, 0xffff, 0);
  if (Buffer.TYPED_ARRAY_SUPPORT) {
    this[offset] = (value >>> 8);
    this[offset + 1] = (value & 0xff);
  } else {
    objectWriteUInt16(this, value, offset, false);
  }
  return offset + 2
};

function objectWriteUInt32 (buf, value, offset, littleEndian) {
  if (value < 0) value = 0xffffffff + value + 1;
  for (var i = 0, j = Math.min(buf.length - offset, 4); i < j; ++i) {
    buf[offset + i] = (value >>> (littleEndian ? i : 3 - i) * 8) & 0xff;
  }
}

Buffer.prototype.writeUInt32LE = function writeUInt32LE (value, offset, noAssert) {
  value = +value;
  offset = offset | 0;
  if (!noAssert) checkInt(this, value, offset, 4, 0xffffffff, 0);
  if (Buffer.TYPED_ARRAY_SUPPORT) {
    this[offset + 3] = (value >>> 24);
    this[offset + 2] = (value >>> 16);
    this[offset + 1] = (value >>> 8);
    this[offset] = (value & 0xff);
  } else {
    objectWriteUInt32(this, value, offset, true);
  }
  return offset + 4
};

Buffer.prototype.writeUInt32BE = function writeUInt32BE (value, offset, noAssert) {
  value = +value;
  offset = offset | 0;
  if (!noAssert) checkInt(this, value, offset, 4, 0xffffffff, 0);
  if (Buffer.TYPED_ARRAY_SUPPORT) {
    this[offset] = (value >>> 24);
    this[offset + 1] = (value >>> 16);
    this[offset + 2] = (value >>> 8);
    this[offset + 3] = (value & 0xff);
  } else {
    objectWriteUInt32(this, value, offset, false);
  }
  return offset + 4
};

Buffer.prototype.writeIntLE = function writeIntLE (value, offset, byteLength, noAssert) {
  value = +value;
  offset = offset | 0;
  if (!noAssert) {
    var limit = Math.pow(2, 8 * byteLength - 1);

    checkInt(this, value, offset, byteLength, limit - 1, -limit);
  }

  var i = 0;
  var mul = 1;
  var sub = 0;
  this[offset] = value & 0xFF;
  while (++i < byteLength && (mul *= 0x100)) {
    if (value < 0 && sub === 0 && this[offset + i - 1] !== 0) {
      sub = 1;
    }
    this[offset + i] = ((value / mul) >> 0) - sub & 0xFF;
  }

  return offset + byteLength
};

Buffer.prototype.writeIntBE = function writeIntBE (value, offset, byteLength, noAssert) {
  value = +value;
  offset = offset | 0;
  if (!noAssert) {
    var limit = Math.pow(2, 8 * byteLength - 1);

    checkInt(this, value, offset, byteLength, limit - 1, -limit);
  }

  var i = byteLength - 1;
  var mul = 1;
  var sub = 0;
  this[offset + i] = value & 0xFF;
  while (--i >= 0 && (mul *= 0x100)) {
    if (value < 0 && sub === 0 && this[offset + i + 1] !== 0) {
      sub = 1;
    }
    this[offset + i] = ((value / mul) >> 0) - sub & 0xFF;
  }

  return offset + byteLength
};

Buffer.prototype.writeInt8 = function writeInt8 (value, offset, noAssert) {
  value = +value;
  offset = offset | 0;
  if (!noAssert) checkInt(this, value, offset, 1, 0x7f, -0x80);
  if (!Buffer.TYPED_ARRAY_SUPPORT) value = Math.floor(value);
  if (value < 0) value = 0xff + value + 1;
  this[offset] = (value & 0xff);
  return offset + 1
};

Buffer.prototype.writeInt16LE = function writeInt16LE (value, offset, noAssert) {
  value = +value;
  offset = offset | 0;
  if (!noAssert) checkInt(this, value, offset, 2, 0x7fff, -0x8000);
  if (Buffer.TYPED_ARRAY_SUPPORT) {
    this[offset] = (value & 0xff);
    this[offset + 1] = (value >>> 8);
  } else {
    objectWriteUInt16(this, value, offset, true);
  }
  return offset + 2
};

Buffer.prototype.writeInt16BE = function writeInt16BE (value, offset, noAssert) {
  value = +value;
  offset = offset | 0;
  if (!noAssert) checkInt(this, value, offset, 2, 0x7fff, -0x8000);
  if (Buffer.TYPED_ARRAY_SUPPORT) {
    this[offset] = (value >>> 8);
    this[offset + 1] = (value & 0xff);
  } else {
    objectWriteUInt16(this, value, offset, false);
  }
  return offset + 2
};

Buffer.prototype.writeInt32LE = function writeInt32LE (value, offset, noAssert) {
  value = +value;
  offset = offset | 0;
  if (!noAssert) checkInt(this, value, offset, 4, 0x7fffffff, -0x80000000);
  if (Buffer.TYPED_ARRAY_SUPPORT) {
    this[offset] = (value & 0xff);
    this[offset + 1] = (value >>> 8);
    this[offset + 2] = (value >>> 16);
    this[offset + 3] = (value >>> 24);
  } else {
    objectWriteUInt32(this, value, offset, true);
  }
  return offset + 4
};

Buffer.prototype.writeInt32BE = function writeInt32BE (value, offset, noAssert) {
  value = +value;
  offset = offset | 0;
  if (!noAssert) checkInt(this, value, offset, 4, 0x7fffffff, -0x80000000);
  if (value < 0) value = 0xffffffff + value + 1;
  if (Buffer.TYPED_ARRAY_SUPPORT) {
    this[offset] = (value >>> 24);
    this[offset + 1] = (value >>> 16);
    this[offset + 2] = (value >>> 8);
    this[offset + 3] = (value & 0xff);
  } else {
    objectWriteUInt32(this, value, offset, false);
  }
  return offset + 4
};

function checkIEEE754 (buf, value, offset, ext, max, min) {
  if (offset + ext > buf.length) throw new RangeError('Index out of range')
  if (offset < 0) throw new RangeError('Index out of range')
}

function writeFloat (buf, value, offset, littleEndian, noAssert) {
  if (!noAssert) {
    checkIEEE754(buf, value, offset, 4);
  }
  write(buf, value, offset, littleEndian, 23, 4);
  return offset + 4
}

Buffer.prototype.writeFloatLE = function writeFloatLE (value, offset, noAssert) {
  return writeFloat(this, value, offset, true, noAssert)
};

Buffer.prototype.writeFloatBE = function writeFloatBE (value, offset, noAssert) {
  return writeFloat(this, value, offset, false, noAssert)
};

function writeDouble (buf, value, offset, littleEndian, noAssert) {
  if (!noAssert) {
    checkIEEE754(buf, value, offset, 8);
  }
  write(buf, value, offset, littleEndian, 52, 8);
  return offset + 8
}

Buffer.prototype.writeDoubleLE = function writeDoubleLE (value, offset, noAssert) {
  return writeDouble(this, value, offset, true, noAssert)
};

Buffer.prototype.writeDoubleBE = function writeDoubleBE (value, offset, noAssert) {
  return writeDouble(this, value, offset, false, noAssert)
};

// copy(targetBuffer, targetStart=0, sourceStart=0, sourceEnd=buffer.length)
Buffer.prototype.copy = function copy (target, targetStart, start, end) {
  if (!start) start = 0;
  if (!end && end !== 0) end = this.length;
  if (targetStart >= target.length) targetStart = target.length;
  if (!targetStart) targetStart = 0;
  if (end > 0 && end < start) end = start;

  // Copy 0 bytes; we're done
  if (end === start) return 0
  if (target.length === 0 || this.length === 0) return 0

  // Fatal error conditions
  if (targetStart < 0) {
    throw new RangeError('targetStart out of bounds')
  }
  if (start < 0 || start >= this.length) throw new RangeError('sourceStart out of bounds')
  if (end < 0) throw new RangeError('sourceEnd out of bounds')

  // Are we oob?
  if (end > this.length) end = this.length;
  if (target.length - targetStart < end - start) {
    end = target.length - targetStart + start;
  }

  var len = end - start;
  var i;

  if (this === target && start < targetStart && targetStart < end) {
    // descending copy from end
    for (i = len - 1; i >= 0; --i) {
      target[i + targetStart] = this[i + start];
    }
  } else if (len < 1000 || !Buffer.TYPED_ARRAY_SUPPORT) {
    // ascending copy from start
    for (i = 0; i < len; ++i) {
      target[i + targetStart] = this[i + start];
    }
  } else {
    Uint8Array.prototype.set.call(
      target,
      this.subarray(start, start + len),
      targetStart
    );
  }

  return len
};

// Usage:
//    buffer.fill(number[, offset[, end]])
//    buffer.fill(buffer[, offset[, end]])
//    buffer.fill(string[, offset[, end]][, encoding])
Buffer.prototype.fill = function fill (val, start, end, encoding) {
  // Handle string cases:
  if (typeof val === 'string') {
    if (typeof start === 'string') {
      encoding = start;
      start = 0;
      end = this.length;
    } else if (typeof end === 'string') {
      encoding = end;
      end = this.length;
    }
    if (val.length === 1) {
      var code = val.charCodeAt(0);
      if (code < 256) {
        val = code;
      }
    }
    if (encoding !== undefined && typeof encoding !== 'string') {
      throw new TypeError('encoding must be a string')
    }
    if (typeof encoding === 'string' && !Buffer.isEncoding(encoding)) {
      throw new TypeError('Unknown encoding: ' + encoding)
    }
  } else if (typeof val === 'number') {
    val = val & 255;
  }

  // Invalid ranges are not set to a default, so can range check early.
  if (start < 0 || this.length < start || this.length < end) {
    throw new RangeError('Out of range index')
  }

  if (end <= start) {
    return this
  }

  start = start >>> 0;
  end = end === undefined ? this.length : end >>> 0;

  if (!val) val = 0;

  var i;
  if (typeof val === 'number') {
    for (i = start; i < end; ++i) {
      this[i] = val;
    }
  } else {
    var bytes = internalIsBuffer(val)
      ? val
      : utf8ToBytes(new Buffer(val, encoding).toString());
    var len = bytes.length;
    for (i = 0; i < end - start; ++i) {
      this[i + start] = bytes[i % len];
    }
  }

  return this
};

// HELPER FUNCTIONS
// ================

var INVALID_BASE64_RE = /[^+\/0-9A-Za-z-_]/g;

function base64clean (str) {
  // Node strips out invalid characters like \n and \t from the string, base64-js does not
  str = stringtrim(str).replace(INVALID_BASE64_RE, '');
  // Node converts strings with length < 2 to ''
  if (str.length < 2) return ''
  // Node allows for non-padded base64 strings (missing trailing ===), base64-js does not
  while (str.length % 4 !== 0) {
    str = str + '=';
  }
  return str
}

function stringtrim (str) {
  if (str.trim) return str.trim()
  return str.replace(/^\s+|\s+$/g, '')
}

function toHex (n) {
  if (n < 16) return '0' + n.toString(16)
  return n.toString(16)
}

function utf8ToBytes (string, units) {
  units = units || Infinity;
  var codePoint;
  var length = string.length;
  var leadSurrogate = null;
  var bytes = [];

  for (var i = 0; i < length; ++i) {
    codePoint = string.charCodeAt(i);

    // is surrogate component
    if (codePoint > 0xD7FF && codePoint < 0xE000) {
      // last char was a lead
      if (!leadSurrogate) {
        // no lead yet
        if (codePoint > 0xDBFF) {
          // unexpected trail
          if ((units -= 3) > -1) bytes.push(0xEF, 0xBF, 0xBD);
          continue
        } else if (i + 1 === length) {
          // unpaired lead
          if ((units -= 3) > -1) bytes.push(0xEF, 0xBF, 0xBD);
          continue
        }

        // valid lead
        leadSurrogate = codePoint;

        continue
      }

      // 2 leads in a row
      if (codePoint < 0xDC00) {
        if ((units -= 3) > -1) bytes.push(0xEF, 0xBF, 0xBD);
        leadSurrogate = codePoint;
        continue
      }

      // valid surrogate pair
      codePoint = (leadSurrogate - 0xD800 << 10 | codePoint - 0xDC00) + 0x10000;
    } else if (leadSurrogate) {
      // valid bmp char, but last char was a lead
      if ((units -= 3) > -1) bytes.push(0xEF, 0xBF, 0xBD);
    }

    leadSurrogate = null;

    // encode utf8
    if (codePoint < 0x80) {
      if ((units -= 1) < 0) break
      bytes.push(codePoint);
    } else if (codePoint < 0x800) {
      if ((units -= 2) < 0) break
      bytes.push(
        codePoint >> 0x6 | 0xC0,
        codePoint & 0x3F | 0x80
      );
    } else if (codePoint < 0x10000) {
      if ((units -= 3) < 0) break
      bytes.push(
        codePoint >> 0xC | 0xE0,
        codePoint >> 0x6 & 0x3F | 0x80,
        codePoint & 0x3F | 0x80
      );
    } else if (codePoint < 0x110000) {
      if ((units -= 4) < 0) break
      bytes.push(
        codePoint >> 0x12 | 0xF0,
        codePoint >> 0xC & 0x3F | 0x80,
        codePoint >> 0x6 & 0x3F | 0x80,
        codePoint & 0x3F | 0x80
      );
    } else {
      throw new Error('Invalid code point')
    }
  }

  return bytes
}

function asciiToBytes (str) {
  var byteArray = [];
  for (var i = 0; i < str.length; ++i) {
    // Node's code seems to be doing this and not & 0x7F..
    byteArray.push(str.charCodeAt(i) & 0xFF);
  }
  return byteArray
}

function utf16leToBytes (str, units) {
  var c, hi, lo;
  var byteArray = [];
  for (var i = 0; i < str.length; ++i) {
    if ((units -= 2) < 0) break

    c = str.charCodeAt(i);
    hi = c >> 8;
    lo = c % 256;
    byteArray.push(lo);
    byteArray.push(hi);
  }

  return byteArray
}


function base64ToBytes (str) {
  return toByteArray(base64clean(str))
}

function blitBuffer (src, dst, offset, length) {
  for (var i = 0; i < length; ++i) {
    if ((i + offset >= dst.length) || (i >= src.length)) break
    dst[i + offset] = src[i];
  }
  return i
}

function isnan (val) {
  return val !== val // eslint-disable-line no-self-compare
}


// the following is from is-buffer, also by Feross Aboukhadijeh and with same lisence
// The _isBuffer check is for Safari 5-7 support, because it's missing
// Object.prototype.constructor. Remove this eventually
function isBuffer(obj) {
  return obj != null && (!!obj._isBuffer || isFastBuffer(obj) || isSlowBuffer(obj))
}

function isFastBuffer (obj) {
  return !!obj.constructor && typeof obj.constructor.isBuffer === 'function' && obj.constructor.isBuffer(obj)
}

// For Node v0.10 support. Remove this eventually.
function isSlowBuffer (obj) {
  return typeof obj.readFloatLE === 'function' && typeof obj.slice === 'function' && isFastBuffer(obj.slice(0, 0))
}

function isNull(arg) {
  return arg === null;
}

function isNullOrUndefined(arg) {
  return arg == null;
}

function isString(arg) {
  return typeof arg === 'string';
}

function isObject(arg) {
  return typeof arg === 'object' && arg !== null;
}

// Copyright Joyent, Inc. and other Node contributors.
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to permit
// persons to whom the Software is furnished to do so, subject to the
// following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN
// NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
// USE OR OTHER DEALINGS IN THE SOFTWARE.


// If obj.hasOwnProperty has been overridden, then calling
// obj.hasOwnProperty(prop) will break.
// See: https://github.com/joyent/node/issues/1707
function hasOwnProperty(obj, prop) {
  return Object.prototype.hasOwnProperty.call(obj, prop);
}
var isArray = Array.isArray || function (xs) {
  return Object.prototype.toString.call(xs) === '[object Array]';
};
function stringifyPrimitive(v) {
  switch (typeof v) {
    case 'string':
      return v;

    case 'boolean':
      return v ? 'true' : 'false';

    case 'number':
      return isFinite(v) ? v : '';

    default:
      return '';
  }
}

function stringify (obj, sep, eq, name) {
  sep = sep || '&';
  eq = eq || '=';
  if (obj === null) {
    obj = undefined;
  }

  if (typeof obj === 'object') {
    return map(objectKeys(obj), function(k) {
      var ks = encodeURIComponent(stringifyPrimitive(k)) + eq;
      if (isArray(obj[k])) {
        return map(obj[k], function(v) {
          return ks + encodeURIComponent(stringifyPrimitive(v));
        }).join(sep);
      } else {
        return ks + encodeURIComponent(stringifyPrimitive(obj[k]));
      }
    }).join(sep);

  }

  if (!name) return '';
  return encodeURIComponent(stringifyPrimitive(name)) + eq +
         encodeURIComponent(stringifyPrimitive(obj));
}
function map (xs, f) {
  if (xs.map) return xs.map(f);
  var res = [];
  for (var i = 0; i < xs.length; i++) {
    res.push(f(xs[i], i));
  }
  return res;
}

var objectKeys = Object.keys || function (obj) {
  var res = [];
  for (var key in obj) {
    if (Object.prototype.hasOwnProperty.call(obj, key)) res.push(key);
  }
  return res;
};

function parse$2(qs, sep, eq, options) {
  sep = sep || '&';
  eq = eq || '=';
  var obj = {};

  if (typeof qs !== 'string' || qs.length === 0) {
    return obj;
  }

  var regexp = /\+/g;
  qs = qs.split(sep);

  var maxKeys = 1000;
  if (options && typeof options.maxKeys === 'number') {
    maxKeys = options.maxKeys;
  }

  var len = qs.length;
  // maxKeys <= 0 means that we should not limit keys count
  if (maxKeys > 0 && len > maxKeys) {
    len = maxKeys;
  }

  for (var i = 0; i < len; ++i) {
    var x = qs[i].replace(regexp, '%20'),
        idx = x.indexOf(eq),
        kstr, vstr, k, v;

    if (idx >= 0) {
      kstr = x.substr(0, idx);
      vstr = x.substr(idx + 1);
    } else {
      kstr = x;
      vstr = '';
    }

    k = decodeURIComponent(kstr);
    v = decodeURIComponent(vstr);

    if (!hasOwnProperty(obj, k)) {
      obj[k] = v;
    } else if (isArray(obj[k])) {
      obj[k].push(v);
    } else {
      obj[k] = [obj[k], v];
    }
  }

  return obj;
}

// Copyright Joyent, Inc. and other Node contributors.
function Url() {
  this.protocol = null;
  this.slashes = null;
  this.auth = null;
  this.host = null;
  this.port = null;
  this.hostname = null;
  this.hash = null;
  this.search = null;
  this.query = null;
  this.pathname = null;
  this.path = null;
  this.href = null;
}

// Reference: RFC 3986, RFC 1808, RFC 2396

// define these here so at least they only have to be
// compiled once on the first module load.
var protocolPattern = /^([a-z0-9.+-]+:)/i,
  portPattern = /:[0-9]*$/,

  // Special case for a simple path URL
  simplePathPattern = /^(\/\/?(?!\/)[^\?\s]*)(\?[^\s]*)?$/,

  // RFC 2396: characters reserved for delimiting URLs.
  // We actually just auto-escape these.
  delims = ['<', '>', '"', '`', ' ', '\r', '\n', '\t'],

  // RFC 2396: characters not allowed for various reasons.
  unwise = ['{', '}', '|', '\\', '^', '`'].concat(delims),

  // Allowed by RFCs, but cause of XSS attacks.  Always escape these.
  autoEscape = ['\''].concat(unwise),
  // Characters that are never ever allowed in a hostname.
  // Note that any invalid chars are also handled, but these
  // are the ones that are *expected* to be seen, so we fast-path
  // them.
  nonHostChars = ['%', '/', '?', ';', '#'].concat(autoEscape),
  hostEndingChars = ['/', '?', '#'],
  hostnameMaxLen = 255,
  hostnamePartPattern = /^[+a-z0-9A-Z_-]{0,63}$/,
  hostnamePartStart = /^([+a-z0-9A-Z_-]{0,63})(.*)$/,
  // protocols that can allow "unsafe" and "unwise" chars.
  unsafeProtocol = {
    'javascript': true,
    'javascript:': true
  },
  // protocols that never have a hostname.
  hostlessProtocol = {
    'javascript': true,
    'javascript:': true
  },
  // protocols that always contain a // bit.
  slashedProtocol = {
    'http': true,
    'https': true,
    'ftp': true,
    'gopher': true,
    'file': true,
    'http:': true,
    'https:': true,
    'ftp:': true,
    'gopher:': true,
    'file:': true
  };

function urlParse(url, parseQueryString, slashesDenoteHost) {
  if (url && isObject(url) && url instanceof Url) return url;

  var u = new Url;
  u.parse(url, parseQueryString, slashesDenoteHost);
  return u;
}
Url.prototype.parse = function(url, parseQueryString, slashesDenoteHost) {
  return parse$1(this, url, parseQueryString, slashesDenoteHost);
};

function parse$1(self, url, parseQueryString, slashesDenoteHost) {
  if (!isString(url)) {
    throw new TypeError('Parameter \'url\' must be a string, not ' + typeof url);
  }

  // Copy chrome, IE, opera backslash-handling behavior.
  // Back slashes before the query string get converted to forward slashes
  // See: https://code.google.com/p/chromium/issues/detail?id=25916
  var queryIndex = url.indexOf('?'),
    splitter =
    (queryIndex !== -1 && queryIndex < url.indexOf('#')) ? '?' : '#',
    uSplit = url.split(splitter),
    slashRegex = /\\/g;
  uSplit[0] = uSplit[0].replace(slashRegex, '/');
  url = uSplit.join(splitter);

  var rest = url;

  // trim before proceeding.
  // This is to support parse stuff like "  http://foo.com  \n"
  rest = rest.trim();

  if (!slashesDenoteHost && url.split('#').length === 1) {
    // Try fast path regexp
    var simplePath = simplePathPattern.exec(rest);
    if (simplePath) {
      self.path = rest;
      self.href = rest;
      self.pathname = simplePath[1];
      if (simplePath[2]) {
        self.search = simplePath[2];
        if (parseQueryString) {
          self.query = parse$2(self.search.substr(1));
        } else {
          self.query = self.search.substr(1);
        }
      } else if (parseQueryString) {
        self.search = '';
        self.query = {};
      }
      return self;
    }
  }

  var proto = protocolPattern.exec(rest);
  if (proto) {
    proto = proto[0];
    var lowerProto = proto.toLowerCase();
    self.protocol = lowerProto;
    rest = rest.substr(proto.length);
  }

  // figure out if it's got a host
  // user@server is *always* interpreted as a hostname, and url
  // resolution will treat //foo/bar as host=foo,path=bar because that's
  // how the browser resolves relative URLs.
  if (slashesDenoteHost || proto || rest.match(/^\/\/[^@\/]+@[^@\/]+/)) {
    var slashes = rest.substr(0, 2) === '//';
    if (slashes && !(proto && hostlessProtocol[proto])) {
      rest = rest.substr(2);
      self.slashes = true;
    }
  }
  var i, hec, l, p;
  if (!hostlessProtocol[proto] &&
    (slashes || (proto && !slashedProtocol[proto]))) {

    // there's a hostname.
    // the first instance of /, ?, ;, or # ends the host.
    //
    // If there is an @ in the hostname, then non-host chars *are* allowed
    // to the left of the last @ sign, unless some host-ending character
    // comes *before* the @-sign.
    // URLs are obnoxious.
    //
    // ex:
    // http://a@b@c/ => user:a@b host:c
    // http://a@b?@c => user:a host:c path:/?@c

    // v0.12 TODO(isaacs): This is not quite how Chrome does things.
    // Review our test case against browsers more comprehensively.

    // find the first instance of any hostEndingChars
    var hostEnd = -1;
    for (i = 0; i < hostEndingChars.length; i++) {
      hec = rest.indexOf(hostEndingChars[i]);
      if (hec !== -1 && (hostEnd === -1 || hec < hostEnd))
        hostEnd = hec;
    }

    // at this point, either we have an explicit point where the
    // auth portion cannot go past, or the last @ char is the decider.
    var auth, atSign;
    if (hostEnd === -1) {
      // atSign can be anywhere.
      atSign = rest.lastIndexOf('@');
    } else {
      // atSign must be in auth portion.
      // http://a@b/c@d => host:b auth:a path:/c@d
      atSign = rest.lastIndexOf('@', hostEnd);
    }

    // Now we have a portion which is definitely the auth.
    // Pull that off.
    if (atSign !== -1) {
      auth = rest.slice(0, atSign);
      rest = rest.slice(atSign + 1);
      self.auth = decodeURIComponent(auth);
    }

    // the host is the remaining to the left of the first non-host char
    hostEnd = -1;
    for (i = 0; i < nonHostChars.length; i++) {
      hec = rest.indexOf(nonHostChars[i]);
      if (hec !== -1 && (hostEnd === -1 || hec < hostEnd))
        hostEnd = hec;
    }
    // if we still have not hit it, then the entire thing is a host.
    if (hostEnd === -1)
      hostEnd = rest.length;

    self.host = rest.slice(0, hostEnd);
    rest = rest.slice(hostEnd);

    // pull out port.
    parseHost(self);

    // we've indicated that there is a hostname,
    // so even if it's empty, it has to be present.
    self.hostname = self.hostname || '';

    // if hostname begins with [ and ends with ]
    // assume that it's an IPv6 address.
    var ipv6Hostname = self.hostname[0] === '[' &&
      self.hostname[self.hostname.length - 1] === ']';

    // validate a little.
    if (!ipv6Hostname) {
      var hostparts = self.hostname.split(/\./);
      for (i = 0, l = hostparts.length; i < l; i++) {
        var part = hostparts[i];
        if (!part) continue;
        if (!part.match(hostnamePartPattern)) {
          var newpart = '';
          for (var j = 0, k = part.length; j < k; j++) {
            if (part.charCodeAt(j) > 127) {
              // we replace non-ASCII char with a temporary placeholder
              // we need this to make sure size of hostname is not
              // broken by replacing non-ASCII by nothing
              newpart += 'x';
            } else {
              newpart += part[j];
            }
          }
          // we test again with ASCII char only
          if (!newpart.match(hostnamePartPattern)) {
            var validParts = hostparts.slice(0, i);
            var notHost = hostparts.slice(i + 1);
            var bit = part.match(hostnamePartStart);
            if (bit) {
              validParts.push(bit[1]);
              notHost.unshift(bit[2]);
            }
            if (notHost.length) {
              rest = '/' + notHost.join('.') + rest;
            }
            self.hostname = validParts.join('.');
            break;
          }
        }
      }
    }

    if (self.hostname.length > hostnameMaxLen) {
      self.hostname = '';
    } else {
      // hostnames are always lower case.
      self.hostname = self.hostname.toLowerCase();
    }

    if (!ipv6Hostname) {
      // IDNA Support: Returns a punycoded representation of "domain".
      // It only converts parts of the domain name that
      // have non-ASCII characters, i.e. it doesn't matter if
      // you call it with a domain that already is ASCII-only.
      self.hostname = toASCII(self.hostname);
    }

    p = self.port ? ':' + self.port : '';
    var h = self.hostname || '';
    self.host = h + p;
    self.href += self.host;

    // strip [ and ] from the hostname
    // the host field still retains them, though
    if (ipv6Hostname) {
      self.hostname = self.hostname.substr(1, self.hostname.length - 2);
      if (rest[0] !== '/') {
        rest = '/' + rest;
      }
    }
  }

  // now rest is set to the post-host stuff.
  // chop off any delim chars.
  if (!unsafeProtocol[lowerProto]) {

    // First, make 100% sure that any "autoEscape" chars get
    // escaped, even if encodeURIComponent doesn't think they
    // need to be.
    for (i = 0, l = autoEscape.length; i < l; i++) {
      var ae = autoEscape[i];
      if (rest.indexOf(ae) === -1)
        continue;
      var esc = encodeURIComponent(ae);
      if (esc === ae) {
        esc = escape(ae);
      }
      rest = rest.split(ae).join(esc);
    }
  }


  // chop off from the tail first.
  var hash = rest.indexOf('#');
  if (hash !== -1) {
    // got a fragment string.
    self.hash = rest.substr(hash);
    rest = rest.slice(0, hash);
  }
  var qm = rest.indexOf('?');
  if (qm !== -1) {
    self.search = rest.substr(qm);
    self.query = rest.substr(qm + 1);
    if (parseQueryString) {
      self.query = parse$2(self.query);
    }
    rest = rest.slice(0, qm);
  } else if (parseQueryString) {
    // no query string, but parseQueryString still requested
    self.search = '';
    self.query = {};
  }
  if (rest) self.pathname = rest;
  if (slashedProtocol[lowerProto] &&
    self.hostname && !self.pathname) {
    self.pathname = '/';
  }

  //to support http.request
  if (self.pathname || self.search) {
    p = self.pathname || '';
    var s = self.search || '';
    self.path = p + s;
  }

  // finally, reconstruct the href based on what has been validated.
  self.href = format(self);
  return self;
}

function format(self) {
  var auth = self.auth || '';
  if (auth) {
    auth = encodeURIComponent(auth);
    auth = auth.replace(/%3A/i, ':');
    auth += '@';
  }

  var protocol = self.protocol || '',
    pathname = self.pathname || '',
    hash = self.hash || '',
    host = false,
    query = '';

  if (self.host) {
    host = auth + self.host;
  } else if (self.hostname) {
    host = auth + (self.hostname.indexOf(':') === -1 ?
      self.hostname :
      '[' + this.hostname + ']');
    if (self.port) {
      host += ':' + self.port;
    }
  }

  if (self.query &&
    isObject(self.query) &&
    Object.keys(self.query).length) {
    query = stringify(self.query);
  }

  var search = self.search || (query && ('?' + query)) || '';

  if (protocol && protocol.substr(-1) !== ':') protocol += ':';

  // only the slashedProtocols get the //.  Not mailto:, xmpp:, etc.
  // unless they had them to begin with.
  if (self.slashes ||
    (!protocol || slashedProtocol[protocol]) && host !== false) {
    host = '//' + (host || '');
    if (pathname && pathname.charAt(0) !== '/') pathname = '/' + pathname;
  } else if (!host) {
    host = '';
  }

  if (hash && hash.charAt(0) !== '#') hash = '#' + hash;
  if (search && search.charAt(0) !== '?') search = '?' + search;

  pathname = pathname.replace(/[?#]/g, function(match) {
    return encodeURIComponent(match);
  });
  search = search.replace('#', '%23');

  return protocol + host + pathname + search + hash;
}

Url.prototype.format = function() {
  return format(this);
};

Url.prototype.resolve = function(relative) {
  return this.resolveObject(urlParse(relative, false, true)).format();
};

Url.prototype.resolveObject = function(relative) {
  if (isString(relative)) {
    var rel = new Url();
    rel.parse(relative, false, true);
    relative = rel;
  }

  var result = new Url();
  var tkeys = Object.keys(this);
  for (var tk = 0; tk < tkeys.length; tk++) {
    var tkey = tkeys[tk];
    result[tkey] = this[tkey];
  }

  // hash is always overridden, no matter what.
  // even href="" will remove it.
  result.hash = relative.hash;

  // if the relative url is empty, then there's nothing left to do here.
  if (relative.href === '') {
    result.href = result.format();
    return result;
  }

  // hrefs like //foo/bar always cut to the protocol.
  if (relative.slashes && !relative.protocol) {
    // take everything except the protocol from relative
    var rkeys = Object.keys(relative);
    for (var rk = 0; rk < rkeys.length; rk++) {
      var rkey = rkeys[rk];
      if (rkey !== 'protocol')
        result[rkey] = relative[rkey];
    }

    //urlParse appends trailing / to urls like http://www.example.com
    if (slashedProtocol[result.protocol] &&
      result.hostname && !result.pathname) {
      result.path = result.pathname = '/';
    }

    result.href = result.format();
    return result;
  }
  var relPath;
  if (relative.protocol && relative.protocol !== result.protocol) {
    // if it's a known url protocol, then changing
    // the protocol does weird things
    // first, if it's not file:, then we MUST have a host,
    // and if there was a path
    // to begin with, then we MUST have a path.
    // if it is file:, then the host is dropped,
    // because that's known to be hostless.
    // anything else is assumed to be absolute.
    if (!slashedProtocol[relative.protocol]) {
      var keys = Object.keys(relative);
      for (var v = 0; v < keys.length; v++) {
        var k = keys[v];
        result[k] = relative[k];
      }
      result.href = result.format();
      return result;
    }

    result.protocol = relative.protocol;
    if (!relative.host && !hostlessProtocol[relative.protocol]) {
      relPath = (relative.pathname || '').split('/');
      while (relPath.length && !(relative.host = relPath.shift()));
      if (!relative.host) relative.host = '';
      if (!relative.hostname) relative.hostname = '';
      if (relPath[0] !== '') relPath.unshift('');
      if (relPath.length < 2) relPath.unshift('');
      result.pathname = relPath.join('/');
    } else {
      result.pathname = relative.pathname;
    }
    result.search = relative.search;
    result.query = relative.query;
    result.host = relative.host || '';
    result.auth = relative.auth;
    result.hostname = relative.hostname || relative.host;
    result.port = relative.port;
    // to support http.request
    if (result.pathname || result.search) {
      var p = result.pathname || '';
      var s = result.search || '';
      result.path = p + s;
    }
    result.slashes = result.slashes || relative.slashes;
    result.href = result.format();
    return result;
  }

  var isSourceAbs = (result.pathname && result.pathname.charAt(0) === '/'),
    isRelAbs = (
      relative.host ||
      relative.pathname && relative.pathname.charAt(0) === '/'
    ),
    mustEndAbs = (isRelAbs || isSourceAbs ||
      (result.host && relative.pathname)),
    removeAllDots = mustEndAbs,
    srcPath = result.pathname && result.pathname.split('/') || [],
    psychotic = result.protocol && !slashedProtocol[result.protocol];
  relPath = relative.pathname && relative.pathname.split('/') || [];
  // if the url is a non-slashed url, then relative
  // links like ../.. should be able
  // to crawl up to the hostname, as well.  This is strange.
  // result.protocol has already been set by now.
  // Later on, put the first path part into the host field.
  if (psychotic) {
    result.hostname = '';
    result.port = null;
    if (result.host) {
      if (srcPath[0] === '') srcPath[0] = result.host;
      else srcPath.unshift(result.host);
    }
    result.host = '';
    if (relative.protocol) {
      relative.hostname = null;
      relative.port = null;
      if (relative.host) {
        if (relPath[0] === '') relPath[0] = relative.host;
        else relPath.unshift(relative.host);
      }
      relative.host = null;
    }
    mustEndAbs = mustEndAbs && (relPath[0] === '' || srcPath[0] === '');
  }
  var authInHost;
  if (isRelAbs) {
    // it's absolute.
    result.host = (relative.host || relative.host === '') ?
      relative.host : result.host;
    result.hostname = (relative.hostname || relative.hostname === '') ?
      relative.hostname : result.hostname;
    result.search = relative.search;
    result.query = relative.query;
    srcPath = relPath;
    // fall through to the dot-handling below.
  } else if (relPath.length) {
    // it's relative
    // throw away the existing file, and take the new path instead.
    if (!srcPath) srcPath = [];
    srcPath.pop();
    srcPath = srcPath.concat(relPath);
    result.search = relative.search;
    result.query = relative.query;
  } else if (!isNullOrUndefined(relative.search)) {
    // just pull out the search.
    // like href='?foo'.
    // Put this after the other two cases because it simplifies the booleans
    if (psychotic) {
      result.hostname = result.host = srcPath.shift();
      //occationaly the auth can get stuck only in host
      //this especially happens in cases like
      //url.resolveObject('mailto:local1@domain1', 'local2@domain2')
      authInHost = result.host && result.host.indexOf('@') > 0 ?
        result.host.split('@') : false;
      if (authInHost) {
        result.auth = authInHost.shift();
        result.host = result.hostname = authInHost.shift();
      }
    }
    result.search = relative.search;
    result.query = relative.query;
    //to support http.request
    if (!isNull(result.pathname) || !isNull(result.search)) {
      result.path = (result.pathname ? result.pathname : '') +
        (result.search ? result.search : '');
    }
    result.href = result.format();
    return result;
  }

  if (!srcPath.length) {
    // no path at all.  easy.
    // we've already handled the other stuff above.
    result.pathname = null;
    //to support http.request
    if (result.search) {
      result.path = '/' + result.search;
    } else {
      result.path = null;
    }
    result.href = result.format();
    return result;
  }

  // if a url ENDs in . or .., then it must get a trailing slash.
  // however, if it ends in anything else non-slashy,
  // then it must NOT get a trailing slash.
  var last = srcPath.slice(-1)[0];
  var hasTrailingSlash = (
    (result.host || relative.host || srcPath.length > 1) &&
    (last === '.' || last === '..') || last === '');

  // strip single dots, resolve double dots to parent dir
  // if the path tries to go above the root, `up` ends up > 0
  var up = 0;
  for (var i = srcPath.length; i >= 0; i--) {
    last = srcPath[i];
    if (last === '.') {
      srcPath.splice(i, 1);
    } else if (last === '..') {
      srcPath.splice(i, 1);
      up++;
    } else if (up) {
      srcPath.splice(i, 1);
      up--;
    }
  }

  // if the path is allowed to go above the root, restore leading ..s
  if (!mustEndAbs && !removeAllDots) {
    for (; up--; up) {
      srcPath.unshift('..');
    }
  }

  if (mustEndAbs && srcPath[0] !== '' &&
    (!srcPath[0] || srcPath[0].charAt(0) !== '/')) {
    srcPath.unshift('');
  }

  if (hasTrailingSlash && (srcPath.join('/').substr(-1) !== '/')) {
    srcPath.push('');
  }

  var isAbsolute = srcPath[0] === '' ||
    (srcPath[0] && srcPath[0].charAt(0) === '/');

  // put the host back
  if (psychotic) {
    result.hostname = result.host = isAbsolute ? '' :
      srcPath.length ? srcPath.shift() : '';
    //occationaly the auth can get stuck only in host
    //this especially happens in cases like
    //url.resolveObject('mailto:local1@domain1', 'local2@domain2')
    authInHost = result.host && result.host.indexOf('@') > 0 ?
      result.host.split('@') : false;
    if (authInHost) {
      result.auth = authInHost.shift();
      result.host = result.hostname = authInHost.shift();
    }
  }

  mustEndAbs = mustEndAbs || (result.host && srcPath.length);

  if (mustEndAbs && !isAbsolute) {
    srcPath.unshift('');
  }

  if (!srcPath.length) {
    result.pathname = null;
    result.path = null;
  } else {
    result.pathname = srcPath.join('/');
  }

  //to support request.http
  if (!isNull(result.pathname) || !isNull(result.search)) {
    result.path = (result.pathname ? result.pathname : '') +
      (result.search ? result.search : '');
  }
  result.auth = relative.auth || result.auth;
  result.slashes = result.slashes || relative.slashes;
  result.href = result.format();
  return result;
};

Url.prototype.parseHost = function() {
  return parseHost(this);
};

function parseHost(self) {
  var host = self.host;
  var port = portPattern.exec(host);
  if (port) {
    port = port[0];
    if (port !== ':') {
      self.port = port.substr(1);
    }
    host = host.substr(0, host.length - port.length);
  }
  if (host) self.hostname = host;
}

var __extends$2 = (undefined && undefined.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
var __awaiter$6 = (undefined && undefined.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator$6 = (undefined && undefined.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
var __read$3 = (undefined && undefined.__read) || function (o, n) {
    var m = typeof Symbol === "function" && o[Symbol.iterator];
    if (!m) return o;
    var i = m.call(o), r, ar = [], e;
    try {
        while ((n === void 0 || n-- > 0) && !(r = i.next()).done) ar.push(r.value);
    }
    catch (error) { e = { error: error }; }
    finally {
        try {
            if (r && !r.done && (m = i["return"])) m.call(i);
        }
        finally { if (e) throw e.error; }
    }
    return ar;
};
var __spread = (undefined && undefined.__spread) || function () {
    for (var ar = [], i = 0; i < arguments.length; i++) ar = ar.concat(__read$3(arguments[i]));
    return ar;
};
var logger$8 = new ConsoleLogger('Util');
var NonRetryableError = /** @class */ (function (_super) {
    __extends$2(NonRetryableError, _super);
    function NonRetryableError(message) {
        var _this = _super.call(this, message) || this;
        _this.nonRetryable = true;
        return _this;
    }
    return NonRetryableError;
}(Error));
var isNonRetryableError$1 = function (obj) {
    var key = 'nonRetryable';
    return obj && obj[key];
};
/**
 * @private
 * Internal use of Amplify only
 */
function retry$1(functionToRetry, args, delayFn, attempt) {
    if (attempt === void 0) { attempt = 1; }
    return __awaiter$6(this, void 0, void 0, function () {
        var err_1, retryIn_1;
        return __generator$6(this, function (_a) {
            switch (_a.label) {
                case 0:
                    if (typeof functionToRetry !== 'function') {
                        throw Error('functionToRetry must be a function');
                    }
                    logger$8.debug(functionToRetry.name + " attempt #" + attempt + " with this vars: " + JSON.stringify(args));
                    _a.label = 1;
                case 1:
                    _a.trys.push([1, 3, , 8]);
                    return [4 /*yield*/, functionToRetry.apply(void 0, __spread(args))];
                case 2: return [2 /*return*/, _a.sent()];
                case 3:
                    err_1 = _a.sent();
                    logger$8.debug("error on " + functionToRetry.name, err_1);
                    if (isNonRetryableError$1(err_1)) {
                        logger$8.debug(functionToRetry.name + " non retryable error", err_1);
                        throw err_1;
                    }
                    retryIn_1 = delayFn(attempt, args, err_1);
                    logger$8.debug(functionToRetry.name + " retrying in " + retryIn_1 + " ms");
                    if (!(retryIn_1 !== false)) return [3 /*break*/, 6];
                    return [4 /*yield*/, new Promise(function (res) { return setTimeout(res, retryIn_1); })];
                case 4:
                    _a.sent();
                    return [4 /*yield*/, retry$1(functionToRetry, args, delayFn, attempt + 1)];
                case 5: return [2 /*return*/, _a.sent()];
                case 6: throw err_1;
                case 7: return [3 /*break*/, 8];
                case 8: return [2 /*return*/];
            }
        });
    });
}
var MAX_DELAY_MS$1 = 5 * 60 * 1000;
/**
 * @private
 * Internal use of Amplify only
 */
function jitteredBackoff$1(maxDelayMs) {
    if (maxDelayMs === void 0) { maxDelayMs = MAX_DELAY_MS$1; }
    var BASE_TIME_MS = 100;
    var JITTER_FACTOR = 100;
    return function (attempt) {
        var delay = Math.pow(2, attempt) * BASE_TIME_MS + JITTER_FACTOR * Math.random();
        return delay > maxDelayMs ? false : delay;
    };
}
/**
 * @private
 * Internal use of Amplify only
 */
var jitteredExponentialRetry$1 = function (functionToRetry, args, maxDelayMs) {
    if (maxDelayMs === void 0) { maxDelayMs = MAX_DELAY_MS$1; }
    return retry$1(functionToRetry, args, jitteredBackoff$1(maxDelayMs));
};

function urlSafeEncode(str) {
    return str
        .split('')
        .map(function (char) {
        return char
            .charCodeAt(0)
            .toString(16)
            .padStart(2, '0');
    })
        .join('');
}
function urlSafeDecode(hex) {
    return hex
        .match(/.{2}/g)
        .map(function (char) { return String.fromCharCode(parseInt(char, 16)); })
        .join('');
}

var __assign$5 = (undefined && undefined.__assign) || function () {
    __assign$5 = Object.assign || function(t) {
        for (var s, i = 1, n = arguments.length; i < n; i++) {
            s = arguments[i];
            for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
                t[p] = s[p];
        }
        return t;
    };
    return __assign$5.apply(this, arguments);
};
var logger$7 = new ConsoleLogger('Parser');
var parseMobileHubConfig = function (config) {
    var amplifyConfig = {};
    // Analytics
    if (config['aws_mobile_analytics_app_id']) {
        var Analytics = {
            AWSPinpoint: {
                appId: config['aws_mobile_analytics_app_id'],
                region: config['aws_mobile_analytics_app_region'],
            },
        };
        amplifyConfig.Analytics = Analytics;
    }
    // Auth
    if (config['aws_cognito_identity_pool_id'] || config['aws_user_pools_id']) {
        amplifyConfig.Auth = {
            userPoolId: config['aws_user_pools_id'],
            userPoolWebClientId: config['aws_user_pools_web_client_id'],
            region: config['aws_cognito_region'],
            identityPoolId: config['aws_cognito_identity_pool_id'],
            identityPoolRegion: config['aws_cognito_region'],
            mandatorySignIn: config['aws_mandatory_sign_in'] === 'enable',
        };
    }
    // Storage
    var storageConfig;
    if (config['aws_user_files_s3_bucket']) {
        storageConfig = {
            AWSS3: {
                bucket: config['aws_user_files_s3_bucket'],
                region: config['aws_user_files_s3_bucket_region'],
                dangerouslyConnectToHttpEndpointForTesting: config['aws_user_files_s3_dangerously_connect_to_http_endpoint_for_testing'],
            },
        };
    }
    else {
        storageConfig = config ? config.Storage || config : {};
    }
    // Logging
    if (config['Logging']) {
        amplifyConfig.Logging = __assign$5(__assign$5({}, config['Logging']), { region: config['aws_project_region'] });
    }
    // Geo
    if (config['geo']) {
        amplifyConfig.Geo = Object.assign({}, config.geo);
        if (config.geo['amazon_location_service']) {
            amplifyConfig.Geo = {
                AmazonLocationService: config.geo['amazon_location_service'],
            };
        }
    }
    amplifyConfig.Analytics = Object.assign({}, amplifyConfig.Analytics, config.Analytics);
    amplifyConfig.Auth = Object.assign({}, amplifyConfig.Auth, config.Auth);
    amplifyConfig.Storage = Object.assign({}, storageConfig);
    amplifyConfig.Logging = Object.assign({}, amplifyConfig.Logging, config.Logging);
    logger$7.debug('parse config', config, 'to amplifyconfig', amplifyConfig);
    return amplifyConfig;
};
/**
 * @deprecated use per-function export
 */
var Parser$1 = /** @class */ (function () {
    function Parser() {
    }
    Parser.parseMobilehubConfig = parseMobileHubConfig;
    return Parser;
}());

var isEmptyData_1$1 = createCommonjsModule(function (module, exports) {
Object.defineProperty(exports, "__esModule", { value: true });
exports.isEmptyData = void 0;
function isEmptyData(data) {
    if (typeof data === "string") {
        return data.length === 0;
    }
    return data.byteLength === 0;
}
exports.isEmptyData = isEmptyData;

});

var constants$1 = createCommonjsModule(function (module, exports) {
Object.defineProperty(exports, "__esModule", { value: true });
exports.EMPTY_DATA_SHA_256 = exports.SHA_256_HMAC_ALGO = exports.SHA_256_HASH = void 0;
exports.SHA_256_HASH = { name: "SHA-256" };
exports.SHA_256_HMAC_ALGO = {
    name: "HMAC",
    hash: exports.SHA_256_HASH
};
exports.EMPTY_DATA_SHA_256 = new Uint8Array([
    227,
    176,
    196,
    66,
    152,
    252,
    28,
    20,
    154,
    251,
    244,
    200,
    153,
    111,
    185,
    36,
    39,
    174,
    65,
    228,
    100,
    155,
    147,
    76,
    164,
    149,
    153,
    27,
    120,
    82,
    184,
    85
]);

});

/**
 * Converts a JS string from its native UCS-2/UTF-16 representation into a
 * Uint8Array of the bytes used to represent the equivalent characters in UTF-8.
 *
 * Cribbed from the `goog.crypt.stringToUtf8ByteArray` function in the Google
 * Closure library, though updated to use typed arrays.
 */
var fromUtf8$2 = function (input) {
    var bytes = [];
    for (var i = 0, len = input.length; i < len; i++) {
        var value = input.charCodeAt(i);
        if (value < 0x80) {
            bytes.push(value);
        }
        else if (value < 0x800) {
            bytes.push((value >> 6) | 192, (value & 63) | 128);
        }
        else if (i + 1 < input.length && (value & 0xfc00) === 0xd800 && (input.charCodeAt(i + 1) & 0xfc00) === 0xdc00) {
            var surrogatePair = 0x10000 + ((value & 1023) << 10) + (input.charCodeAt(++i) & 1023);
            bytes.push((surrogatePair >> 18) | 240, ((surrogatePair >> 12) & 63) | 128, ((surrogatePair >> 6) & 63) | 128, (surrogatePair & 63) | 128);
        }
        else {
            bytes.push((value >> 12) | 224, ((value >> 6) & 63) | 128, (value & 63) | 128);
        }
    }
    return Uint8Array.from(bytes);
};
/**
 * Converts a typed array of bytes containing UTF-8 data into a native JS
 * string.
 *
 * Partly cribbed from the `goog.crypt.utf8ByteArrayToString` function in the
 * Google Closure library, though updated to use typed arrays and to better
 * handle astral plane code points.
 */
var toUtf8$2 = function (input) {
    var decoded = "";
    for (var i = 0, len = input.length; i < len; i++) {
        var byte = input[i];
        if (byte < 0x80) {
            decoded += String.fromCharCode(byte);
        }
        else if (192 <= byte && byte < 224) {
            var nextByte = input[++i];
            decoded += String.fromCharCode(((byte & 31) << 6) | (nextByte & 63));
        }
        else if (240 <= byte && byte < 365) {
            var surrogatePair = [byte, input[++i], input[++i], input[++i]];
            var encoded = "%" + surrogatePair.map(function (byteValue) { return byteValue.toString(16); }).join("%");
            decoded += decodeURIComponent(encoded);
        }
        else {
            decoded += String.fromCharCode(((byte & 15) << 12) | ((input[++i] & 63) << 6) | (input[++i] & 63));
        }
    }
    return decoded;
};

function fromUtf8$1(input) {
    return new TextEncoder().encode(input);
}
function toUtf8$1(input) {
    return new TextDecoder("utf-8").decode(input);
}

var fromUtf8 = function (input) {
    return typeof TextEncoder === "function" ? fromUtf8$1(input) : fromUtf8$2(input);
};
var toUtf8 = function (input) {
    return typeof TextDecoder === "function" ? toUtf8$1(input) : toUtf8$2(input);
};

var es = /*#__PURE__*/Object.freeze({
    __proto__: null,
    fromUtf8: fromUtf8,
    toUtf8: toUtf8
});

var fallbackWindow = {};
function locateWindow() {
    if (typeof window !== "undefined") {
        return window;
    }
    else if (typeof self !== "undefined") {
        return self;
    }
    return fallbackWindow;
}

var distEs = /*#__PURE__*/Object.freeze({
    __proto__: null,
    locateWindow: locateWindow
});

var ie11Sha256 = createCommonjsModule(function (module, exports) {
Object.defineProperty(exports, "__esModule", { value: true });
exports.Sha256 = void 0;




var Sha256 = /** @class */ (function () {
    function Sha256(secret) {
        if (secret) {
            this.operation = getKeyPromise(secret).then(function (keyData) {
                return (0, distEs.locateWindow)().msCrypto.subtle.sign(constants$1.SHA_256_HMAC_ALGO, keyData);
            });
            this.operation.catch(function () { });
        }
        else {
            this.operation = Promise.resolve((0, distEs.locateWindow)().msCrypto.subtle.digest("SHA-256"));
        }
    }
    Sha256.prototype.update = function (toHash) {
        var _this = this;
        if ((0, isEmptyData_1$1.isEmptyData)(toHash)) {
            return;
        }
        this.operation = this.operation.then(function (operation) {
            operation.onerror = function () {
                _this.operation = Promise.reject(new Error("Error encountered updating hash"));
            };
            operation.process(toArrayBufferView(toHash));
            return operation;
        });
        this.operation.catch(function () { });
    };
    Sha256.prototype.digest = function () {
        return this.operation.then(function (operation) {
            return new Promise(function (resolve, reject) {
                operation.onerror = function () {
                    reject(new Error("Error encountered finalizing hash"));
                };
                operation.oncomplete = function () {
                    if (operation.result) {
                        resolve(new Uint8Array(operation.result));
                    }
                    reject(new Error("Error encountered finalizing hash"));
                };
                operation.finish();
            });
        });
    };
    return Sha256;
}());
exports.Sha256 = Sha256;
function getKeyPromise(secret) {
    return new Promise(function (resolve, reject) {
        var keyOperation = (0, distEs.locateWindow)().msCrypto.subtle.importKey("raw", toArrayBufferView(secret), constants$1.SHA_256_HMAC_ALGO, false, ["sign"]);
        keyOperation.oncomplete = function () {
            if (keyOperation.result) {
                resolve(keyOperation.result);
            }
            reject(new Error("ImportKey completed without importing key."));
        };
        keyOperation.onerror = function () {
            reject(new Error("ImportKey failed to import key."));
        };
    });
}
function toArrayBufferView(data) {
    if (typeof data === "string") {
        return (0, es.fromUtf8)(data);
    }
    if (ArrayBuffer.isView(data)) {
        return new Uint8Array(data.buffer, data.byteOffset, data.byteLength / Uint8Array.BYTES_PER_ELEMENT);
    }
    return new Uint8Array(data);
}

});

var convertToBuffer_1 = createCommonjsModule(function (module, exports) {
// Copyright Amazon.com Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0
Object.defineProperty(exports, "__esModule", { value: true });
exports.convertToBuffer = void 0;

// Quick polyfill
var fromUtf8 = typeof Buffer !== "undefined" && Buffer.from
    ? function (input) { return Buffer.from(input, "utf8"); }
    : es.fromUtf8;
function convertToBuffer(data) {
    // Already a Uint8, do nothing
    if (data instanceof Uint8Array)
        return data;
    if (typeof data === "string") {
        return fromUtf8(data);
    }
    if (ArrayBuffer.isView(data)) {
        return new Uint8Array(data.buffer, data.byteOffset, data.byteLength / Uint8Array.BYTES_PER_ELEMENT);
    }
    return new Uint8Array(data);
}
exports.convertToBuffer = convertToBuffer;

});

var isEmptyData_1 = createCommonjsModule(function (module, exports) {
// Copyright Amazon.com Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0
Object.defineProperty(exports, "__esModule", { value: true });
exports.isEmptyData = void 0;
function isEmptyData(data) {
    if (typeof data === "string") {
        return data.length === 0;
    }
    return data.byteLength === 0;
}
exports.isEmptyData = isEmptyData;

});

var numToUint8_1 = createCommonjsModule(function (module, exports) {
// Copyright Amazon.com Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0
Object.defineProperty(exports, "__esModule", { value: true });
exports.numToUint8 = void 0;
function numToUint8(num) {
    return new Uint8Array([
        (num & 0xff000000) >> 24,
        (num & 0x00ff0000) >> 16,
        (num & 0x0000ff00) >> 8,
        num & 0x000000ff,
    ]);
}
exports.numToUint8 = numToUint8;

});

var uint32ArrayFrom_1 = createCommonjsModule(function (module, exports) {
// Copyright Amazon.com Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0
Object.defineProperty(exports, "__esModule", { value: true });
exports.uint32ArrayFrom = void 0;
// IE 11 does not support Array.from, so we do it manually
function uint32ArrayFrom(a_lookUpTable) {
    if (!Array.from) {
        var return_array = new Uint32Array(a_lookUpTable.length);
        var a_index = 0;
        while (a_index < a_lookUpTable.length) {
            return_array[a_index] = a_lookUpTable[a_index];
        }
        return return_array;
    }
    return Uint32Array.from(a_lookUpTable);
}
exports.uint32ArrayFrom = uint32ArrayFrom;

});

var build$4 = createCommonjsModule(function (module, exports) {
// Copyright Amazon.com Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0
Object.defineProperty(exports, "__esModule", { value: true });
exports.uint32ArrayFrom = exports.numToUint8 = exports.isEmptyData = exports.convertToBuffer = void 0;

Object.defineProperty(exports, "convertToBuffer", { enumerable: true, get: function () { return convertToBuffer_1.convertToBuffer; } });

Object.defineProperty(exports, "isEmptyData", { enumerable: true, get: function () { return isEmptyData_1.isEmptyData; } });

Object.defineProperty(exports, "numToUint8", { enumerable: true, get: function () { return numToUint8_1.numToUint8; } });

Object.defineProperty(exports, "uint32ArrayFrom", { enumerable: true, get: function () { return uint32ArrayFrom_1.uint32ArrayFrom; } });

});

var webCryptoSha256 = createCommonjsModule(function (module, exports) {
Object.defineProperty(exports, "__esModule", { value: true });
exports.Sha256 = void 0;



var Sha256 = /** @class */ (function () {
    function Sha256(secret) {
        this.toHash = new Uint8Array(0);
        if (secret !== void 0) {
            this.key = new Promise(function (resolve, reject) {
                (0, distEs.locateWindow)()
                    .crypto.subtle.importKey("raw", (0, build$4.convertToBuffer)(secret), constants$1.SHA_256_HMAC_ALGO, false, ["sign"])
                    .then(resolve, reject);
            });
            this.key.catch(function () { });
        }
    }
    Sha256.prototype.update = function (data) {
        if ((0, build$4.isEmptyData)(data)) {
            return;
        }
        var update = (0, build$4.convertToBuffer)(data);
        var typedArray = new Uint8Array(this.toHash.byteLength + update.byteLength);
        typedArray.set(this.toHash, 0);
        typedArray.set(update, this.toHash.byteLength);
        this.toHash = typedArray;
    };
    Sha256.prototype.digest = function () {
        var _this = this;
        if (this.key) {
            return this.key.then(function (key) {
                return (0, distEs.locateWindow)()
                    .crypto.subtle.sign(constants$1.SHA_256_HMAC_ALGO, key, _this.toHash)
                    .then(function (data) { return new Uint8Array(data); });
            });
        }
        if ((0, build$4.isEmptyData)(this.toHash)) {
            return Promise.resolve(constants$1.EMPTY_DATA_SHA_256);
        }
        return Promise.resolve()
            .then(function () {
            return (0, distEs.locateWindow)().crypto.subtle.digest(constants$1.SHA_256_HASH, _this.toHash);
        })
            .then(function (data) { return Promise.resolve(new Uint8Array(data)); });
    };
    return Sha256;
}());
exports.Sha256 = Sha256;

});

var constants = createCommonjsModule(function (module, exports) {
Object.defineProperty(exports, "__esModule", { value: true });
exports.MAX_HASHABLE_LENGTH = exports.INIT = exports.KEY = exports.DIGEST_LENGTH = exports.BLOCK_SIZE = void 0;
/**
 * @internal
 */
exports.BLOCK_SIZE = 64;
/**
 * @internal
 */
exports.DIGEST_LENGTH = 32;
/**
 * @internal
 */
exports.KEY = new Uint32Array([
    0x428a2f98,
    0x71374491,
    0xb5c0fbcf,
    0xe9b5dba5,
    0x3956c25b,
    0x59f111f1,
    0x923f82a4,
    0xab1c5ed5,
    0xd807aa98,
    0x12835b01,
    0x243185be,
    0x550c7dc3,
    0x72be5d74,
    0x80deb1fe,
    0x9bdc06a7,
    0xc19bf174,
    0xe49b69c1,
    0xefbe4786,
    0x0fc19dc6,
    0x240ca1cc,
    0x2de92c6f,
    0x4a7484aa,
    0x5cb0a9dc,
    0x76f988da,
    0x983e5152,
    0xa831c66d,
    0xb00327c8,
    0xbf597fc7,
    0xc6e00bf3,
    0xd5a79147,
    0x06ca6351,
    0x14292967,
    0x27b70a85,
    0x2e1b2138,
    0x4d2c6dfc,
    0x53380d13,
    0x650a7354,
    0x766a0abb,
    0x81c2c92e,
    0x92722c85,
    0xa2bfe8a1,
    0xa81a664b,
    0xc24b8b70,
    0xc76c51a3,
    0xd192e819,
    0xd6990624,
    0xf40e3585,
    0x106aa070,
    0x19a4c116,
    0x1e376c08,
    0x2748774c,
    0x34b0bcb5,
    0x391c0cb3,
    0x4ed8aa4a,
    0x5b9cca4f,
    0x682e6ff3,
    0x748f82ee,
    0x78a5636f,
    0x84c87814,
    0x8cc70208,
    0x90befffa,
    0xa4506ceb,
    0xbef9a3f7,
    0xc67178f2
]);
/**
 * @internal
 */
exports.INIT = [
    0x6a09e667,
    0xbb67ae85,
    0x3c6ef372,
    0xa54ff53a,
    0x510e527f,
    0x9b05688c,
    0x1f83d9ab,
    0x5be0cd19
];
/**
 * @internal
 */
exports.MAX_HASHABLE_LENGTH = Math.pow(2, 53) - 1;

});

var RawSha256_1 = createCommonjsModule(function (module, exports) {
Object.defineProperty(exports, "__esModule", { value: true });
exports.RawSha256 = void 0;

/**
 * @internal
 */
var RawSha256 = /** @class */ (function () {
    function RawSha256() {
        this.state = Int32Array.from(constants.INIT);
        this.temp = new Int32Array(64);
        this.buffer = new Uint8Array(64);
        this.bufferLength = 0;
        this.bytesHashed = 0;
        /**
         * @internal
         */
        this.finished = false;
    }
    RawSha256.prototype.update = function (data) {
        if (this.finished) {
            throw new Error("Attempted to update an already finished hash.");
        }
        var position = 0;
        var byteLength = data.byteLength;
        this.bytesHashed += byteLength;
        if (this.bytesHashed * 8 > constants.MAX_HASHABLE_LENGTH) {
            throw new Error("Cannot hash more than 2^53 - 1 bits");
        }
        while (byteLength > 0) {
            this.buffer[this.bufferLength++] = data[position++];
            byteLength--;
            if (this.bufferLength === constants.BLOCK_SIZE) {
                this.hashBuffer();
                this.bufferLength = 0;
            }
        }
    };
    RawSha256.prototype.digest = function () {
        if (!this.finished) {
            var bitsHashed = this.bytesHashed * 8;
            var bufferView = new DataView(this.buffer.buffer, this.buffer.byteOffset, this.buffer.byteLength);
            var undecoratedLength = this.bufferLength;
            bufferView.setUint8(this.bufferLength++, 0x80);
            // Ensure the final block has enough room for the hashed length
            if (undecoratedLength % constants.BLOCK_SIZE >= constants.BLOCK_SIZE - 8) {
                for (var i = this.bufferLength; i < constants.BLOCK_SIZE; i++) {
                    bufferView.setUint8(i, 0);
                }
                this.hashBuffer();
                this.bufferLength = 0;
            }
            for (var i = this.bufferLength; i < constants.BLOCK_SIZE - 8; i++) {
                bufferView.setUint8(i, 0);
            }
            bufferView.setUint32(constants.BLOCK_SIZE - 8, Math.floor(bitsHashed / 0x100000000), true);
            bufferView.setUint32(constants.BLOCK_SIZE - 4, bitsHashed);
            this.hashBuffer();
            this.finished = true;
        }
        // The value in state is little-endian rather than big-endian, so flip
        // each word into a new Uint8Array
        var out = new Uint8Array(constants.DIGEST_LENGTH);
        for (var i = 0; i < 8; i++) {
            out[i * 4] = (this.state[i] >>> 24) & 0xff;
            out[i * 4 + 1] = (this.state[i] >>> 16) & 0xff;
            out[i * 4 + 2] = (this.state[i] >>> 8) & 0xff;
            out[i * 4 + 3] = (this.state[i] >>> 0) & 0xff;
        }
        return out;
    };
    RawSha256.prototype.hashBuffer = function () {
        var _a = this, buffer = _a.buffer, state = _a.state;
        var state0 = state[0], state1 = state[1], state2 = state[2], state3 = state[3], state4 = state[4], state5 = state[5], state6 = state[6], state7 = state[7];
        for (var i = 0; i < constants.BLOCK_SIZE; i++) {
            if (i < 16) {
                this.temp[i] =
                    ((buffer[i * 4] & 0xff) << 24) |
                        ((buffer[i * 4 + 1] & 0xff) << 16) |
                        ((buffer[i * 4 + 2] & 0xff) << 8) |
                        (buffer[i * 4 + 3] & 0xff);
            }
            else {
                var u = this.temp[i - 2];
                var t1_1 = ((u >>> 17) | (u << 15)) ^ ((u >>> 19) | (u << 13)) ^ (u >>> 10);
                u = this.temp[i - 15];
                var t2_1 = ((u >>> 7) | (u << 25)) ^ ((u >>> 18) | (u << 14)) ^ (u >>> 3);
                this.temp[i] =
                    ((t1_1 + this.temp[i - 7]) | 0) + ((t2_1 + this.temp[i - 16]) | 0);
            }
            var t1 = ((((((state4 >>> 6) | (state4 << 26)) ^
                ((state4 >>> 11) | (state4 << 21)) ^
                ((state4 >>> 25) | (state4 << 7))) +
                ((state4 & state5) ^ (~state4 & state6))) |
                0) +
                ((state7 + ((constants.KEY[i] + this.temp[i]) | 0)) | 0)) |
                0;
            var t2 = ((((state0 >>> 2) | (state0 << 30)) ^
                ((state0 >>> 13) | (state0 << 19)) ^
                ((state0 >>> 22) | (state0 << 10))) +
                ((state0 & state1) ^ (state0 & state2) ^ (state1 & state2))) |
                0;
            state7 = state6;
            state6 = state5;
            state5 = state4;
            state4 = (state3 + t1) | 0;
            state3 = state2;
            state2 = state1;
            state1 = state0;
            state0 = (t1 + t2) | 0;
        }
        state[0] += state0;
        state[1] += state1;
        state[2] += state2;
        state[3] += state3;
        state[4] += state4;
        state[5] += state5;
        state[6] += state6;
        state[7] += state7;
    };
    return RawSha256;
}());
exports.RawSha256 = RawSha256;

});

var jsSha256 = createCommonjsModule(function (module, exports) {
Object.defineProperty(exports, "__esModule", { value: true });
exports.Sha256 = void 0;




var Sha256 = /** @class */ (function () {
    function Sha256(secret) {
        this.hash = new RawSha256_1.RawSha256();
        if (secret) {
            this.outer = new RawSha256_1.RawSha256();
            var inner = bufferFromSecret(secret);
            var outer = new Uint8Array(constants.BLOCK_SIZE);
            outer.set(inner);
            for (var i = 0; i < constants.BLOCK_SIZE; i++) {
                inner[i] ^= 0x36;
                outer[i] ^= 0x5c;
            }
            this.hash.update(inner);
            this.outer.update(outer);
            // overwrite the copied key in memory
            for (var i = 0; i < inner.byteLength; i++) {
                inner[i] = 0;
            }
        }
    }
    Sha256.prototype.update = function (toHash) {
        if ((0, build$4.isEmptyData)(toHash) || this.error) {
            return;
        }
        try {
            this.hash.update((0, build$4.convertToBuffer)(toHash));
        }
        catch (e) {
            this.error = e;
        }
    };
    /* This synchronous method keeps compatibility
     * with the v2 aws-sdk.
     */
    Sha256.prototype.digestSync = function () {
        if (this.error) {
            throw this.error;
        }
        if (this.outer) {
            if (!this.outer.finished) {
                this.outer.update(this.hash.digest());
            }
            return this.outer.digest();
        }
        return this.hash.digest();
    };
    /* The underlying digest method here is synchronous.
     * To keep the same interface with the other hash functions
     * the default is to expose this as an async method.
     * However, it can sometimes be useful to have a sync method.
     */
    Sha256.prototype.digest = function () {
        return (0, tslib_es6.__awaiter)(this, void 0, void 0, function () {
            return (0, tslib_es6.__generator)(this, function (_a) {
                return [2 /*return*/, this.digestSync()];
            });
        });
    };
    return Sha256;
}());
exports.Sha256 = Sha256;
function bufferFromSecret(secret) {
    var input = (0, build$4.convertToBuffer)(secret);
    if (input.byteLength > constants.BLOCK_SIZE) {
        var bufferHash = new RawSha256_1.RawSha256();
        bufferHash.update(input);
        input = bufferHash.digest();
    }
    var buffer = new Uint8Array(constants.BLOCK_SIZE);
    buffer.set(input);
    return buffer;
}

});

var build$3 = createCommonjsModule(function (module, exports) {
Object.defineProperty(exports, "__esModule", { value: true });

(0, tslib_es6.__exportStar)(jsSha256, exports);

});

var supportsWebCrypto_1 = createCommonjsModule(function (module, exports) {
Object.defineProperty(exports, "__esModule", { value: true });
exports.supportsZeroByteGCM = exports.supportsSubtleCrypto = exports.supportsSecureRandom = exports.supportsWebCrypto = void 0;

var subtleCryptoMethods = [
    "decrypt",
    "digest",
    "encrypt",
    "exportKey",
    "generateKey",
    "importKey",
    "sign",
    "verify"
];
function supportsWebCrypto(window) {
    if (supportsSecureRandom(window) &&
        typeof window.crypto.subtle === "object") {
        var subtle = window.crypto.subtle;
        return supportsSubtleCrypto(subtle);
    }
    return false;
}
exports.supportsWebCrypto = supportsWebCrypto;
function supportsSecureRandom(window) {
    if (typeof window === "object" && typeof window.crypto === "object") {
        var getRandomValues = window.crypto.getRandomValues;
        return typeof getRandomValues === "function";
    }
    return false;
}
exports.supportsSecureRandom = supportsSecureRandom;
function supportsSubtleCrypto(subtle) {
    return (subtle &&
        subtleCryptoMethods.every(function (methodName) { return typeof subtle[methodName] === "function"; }));
}
exports.supportsSubtleCrypto = supportsSubtleCrypto;
function supportsZeroByteGCM(subtle) {
    return tslib_es6.__awaiter(this, void 0, void 0, function () {
        var key, zeroByteAuthTag;
        return tslib_es6.__generator(this, function (_b) {
            switch (_b.label) {
                case 0:
                    if (!supportsSubtleCrypto(subtle))
                        return [2 /*return*/, false];
                    _b.label = 1;
                case 1:
                    _b.trys.push([1, 4, , 5]);
                    return [4 /*yield*/, subtle.generateKey({ name: "AES-GCM", length: 128 }, false, ["encrypt"])];
                case 2:
                    key = _b.sent();
                    return [4 /*yield*/, subtle.encrypt({
                            name: "AES-GCM",
                            iv: new Uint8Array(Array(12)),
                            additionalData: new Uint8Array(Array(16)),
                            tagLength: 128
                        }, key, new Uint8Array(0))];
                case 3:
                    zeroByteAuthTag = _b.sent();
                    return [2 /*return*/, zeroByteAuthTag.byteLength === 16];
                case 4:
                    _b.sent();
                    return [2 /*return*/, false];
                case 5: return [2 /*return*/];
            }
        });
    });
}
exports.supportsZeroByteGCM = supportsZeroByteGCM;

});

var build$2 = createCommonjsModule(function (module, exports) {
Object.defineProperty(exports, "__esModule", { value: true });

tslib_es6.__exportStar(supportsWebCrypto_1, exports);

});

var CryptoOperation = createCommonjsModule(function (module, exports) {
Object.defineProperty(exports, "__esModule", { value: true });

});

var Key = createCommonjsModule(function (module, exports) {
Object.defineProperty(exports, "__esModule", { value: true });

});

var KeyOperation = createCommonjsModule(function (module, exports) {
Object.defineProperty(exports, "__esModule", { value: true });

});

var MsSubtleCrypto = createCommonjsModule(function (module, exports) {
Object.defineProperty(exports, "__esModule", { value: true });

});

var MsWindow = createCommonjsModule(function (module, exports) {
Object.defineProperty(exports, "__esModule", { value: true });
exports.isMsWindow = void 0;
var msSubtleCryptoMethods = [
    "decrypt",
    "digest",
    "encrypt",
    "exportKey",
    "generateKey",
    "importKey",
    "sign",
    "verify"
];
function quacksLikeAnMsWindow(window) {
    return "MSInputMethodContext" in window && "msCrypto" in window;
}
/**
 * Determines if the provided window is (or is like) the window object one would
 * expect to encounter in Internet Explorer 11.
 */
function isMsWindow(window) {
    if (quacksLikeAnMsWindow(window) && window.msCrypto.subtle !== undefined) {
        var _a = window.msCrypto, getRandomValues = _a.getRandomValues, subtle_1 = _a.subtle;
        return msSubtleCryptoMethods
            .map(function (methodName) { return subtle_1[methodName]; })
            .concat(getRandomValues)
            .every(function (method) { return typeof method === "function"; });
    }
    return false;
}
exports.isMsWindow = isMsWindow;

});

var build$1 = createCommonjsModule(function (module, exports) {
Object.defineProperty(exports, "__esModule", { value: true });

tslib_es6.__exportStar(CryptoOperation, exports);
tslib_es6.__exportStar(Key, exports);
tslib_es6.__exportStar(KeyOperation, exports);
tslib_es6.__exportStar(MsSubtleCrypto, exports);
tslib_es6.__exportStar(MsWindow, exports);

});

var crossPlatformSha256 = createCommonjsModule(function (module, exports) {
Object.defineProperty(exports, "__esModule", { value: true });
exports.Sha256 = void 0;






var Sha256 = /** @class */ (function () {
    function Sha256(secret) {
        if ((0, build$2.supportsWebCrypto)((0, distEs.locateWindow)())) {
            this.hash = new webCryptoSha256.Sha256(secret);
        }
        else if ((0, build$1.isMsWindow)((0, distEs.locateWindow)())) {
            this.hash = new ie11Sha256.Sha256(secret);
        }
        else {
            this.hash = new build$3.Sha256(secret);
        }
    }
    Sha256.prototype.update = function (data, encoding) {
        this.hash.update(data, encoding);
    };
    Sha256.prototype.digest = function () {
        return this.hash.digest();
    };
    return Sha256;
}());
exports.Sha256 = Sha256;

});

var build = createCommonjsModule(function (module, exports) {
Object.defineProperty(exports, "__esModule", { value: true });
exports.WebCryptoSha256 = exports.Ie11Sha256 = void 0;

(0, tslib_es6.__exportStar)(crossPlatformSha256, exports);

Object.defineProperty(exports, "Ie11Sha256", { enumerable: true, get: function () { return ie11Sha256.Sha256; } });

Object.defineProperty(exports, "WebCryptoSha256", { enumerable: true, get: function () { return webCryptoSha256.Sha256; } });

});

var HttpResponse = /** @class */ (function () {
    function HttpResponse(options) {
        this.statusCode = options.statusCode;
        this.headers = options.headers || {};
        this.body = options.body;
    }
    HttpResponse.isInstance = function (response) {
        //determine if response is a valid HttpResponse
        if (!response)
            return false;
        var resp = response;
        return typeof resp.statusCode === "number" && typeof resp.headers === "object";
    };
    return HttpResponse;
}());

var HttpRequest = /** @class */ (function () {
    function HttpRequest(options) {
        this.method = options.method || "GET";
        this.hostname = options.hostname || "localhost";
        this.port = options.port;
        this.query = options.query || {};
        this.headers = options.headers || {};
        this.body = options.body;
        this.protocol = options.protocol
            ? options.protocol.substr(-1) !== ":"
                ? options.protocol + ":"
                : options.protocol
            : "https:";
        this.path = options.path ? (options.path.charAt(0) !== "/" ? "/" + options.path : options.path) : "/";
    }
    HttpRequest.isInstance = function (request) {
        //determine if request is a valid httpRequest
        if (!request)
            return false;
        var req = request;
        return ("method" in req &&
            "protocol" in req &&
            "hostname" in req &&
            "path" in req &&
            typeof req["query"] === "object" &&
            typeof req["headers"] === "object");
    };
    HttpRequest.prototype.clone = function () {
        var cloned = new HttpRequest(__assign$6(__assign$6({}, this), { headers: __assign$6({}, this.headers) }));
        if (cloned.query)
            cloned.query = cloneQuery$1(cloned.query);
        return cloned;
    };
    return HttpRequest;
}());
function cloneQuery$1(query) {
    return Object.keys(query).reduce(function (carry, paramName) {
        var _a;
        var param = query[paramName];
        return __assign$6(__assign$6({}, carry), (_a = {}, _a[paramName] = Array.isArray(param) ? __spread$1(param) : param, _a));
    }, {});
}

var escapeUri = function (uri) {
    // AWS percent-encodes some extra non-standard characters in a URI
    return encodeURIComponent(uri).replace(/[!'()*]/g, hexEncode);
};
var hexEncode = function (c) { return "%" + c.charCodeAt(0).toString(16).toUpperCase(); };

function buildQueryString(query) {
    var e_1, _a;
    var parts = [];
    try {
        for (var _b = __values(Object.keys(query).sort()), _c = _b.next(); !_c.done; _c = _b.next()) {
            var key = _c.value;
            var value = query[key];
            key = escapeUri(key);
            if (Array.isArray(value)) {
                for (var i = 0, iLen = value.length; i < iLen; i++) {
                    parts.push(key + "=" + escapeUri(value[i]));
                }
            }
            else {
                var qsEntry = key;
                if (value || typeof value === "string") {
                    qsEntry += "=" + escapeUri(value);
                }
                parts.push(qsEntry);
            }
        }
    }
    catch (e_1_1) { e_1 = { error: e_1_1 }; }
    finally {
        try {
            if (_c && !_c.done && (_a = _b.return)) _a.call(_b);
        }
        finally { if (e_1) throw e_1.error; }
    }
    return parts.join("&");
}

function requestTimeout(timeoutInMs) {
    if (timeoutInMs === void 0) { timeoutInMs = 0; }
    return new Promise(function (resolve, reject) {
        if (timeoutInMs) {
            setTimeout(function () {
                var timeoutError = new Error("Request did not complete within " + timeoutInMs + " ms");
                timeoutError.name = "TimeoutError";
                reject(timeoutError);
            }, timeoutInMs);
        }
    });
}

var FetchHttpHandler = /** @class */ (function () {
    function FetchHttpHandler(_a) {
        var _b = _a === void 0 ? {} : _a, requestTimeout = _b.requestTimeout;
        this.requestTimeout = requestTimeout;
    }
    FetchHttpHandler.prototype.destroy = function () {
        // Do nothing. TLS and HTTP/2 connection pooling is handled by the browser.
    };
    FetchHttpHandler.prototype.handle = function (request, _a) {
        var _b = _a === void 0 ? {} : _a, abortSignal = _b.abortSignal;
        var requestTimeoutInMs = this.requestTimeout;
        // if the request was already aborted, prevent doing extra work
        if (abortSignal === null || abortSignal === void 0 ? void 0 : abortSignal.aborted) {
            var abortError = new Error("Request aborted");
            abortError.name = "AbortError";
            return Promise.reject(abortError);
        }
        var path = request.path;
        if (request.query) {
            var queryString = buildQueryString(request.query);
            if (queryString) {
                path += "?" + queryString;
            }
        }
        var port = request.port, method = request.method;
        var url = request.protocol + "//" + request.hostname + (port ? ":" + port : "") + path;
        // Request constructor doesn't allow GET/HEAD request with body
        // ref: https://github.com/whatwg/fetch/issues/551
        var body = method === "GET" || method === "HEAD" ? undefined : request.body;
        var requestOptions = {
            body: body,
            headers: new Headers(request.headers),
            method: method,
        };
        // some browsers support abort signal
        if (typeof AbortController !== "undefined") {
            requestOptions["signal"] = abortSignal;
        }
        var fetchRequest = new Request(url, requestOptions);
        var raceOfPromises = [
            fetch(fetchRequest).then(function (response) {
                var e_1, _a;
                var fetchHeaders = response.headers;
                var transformedHeaders = {};
                try {
                    for (var _b = __values(fetchHeaders.entries()), _c = _b.next(); !_c.done; _c = _b.next()) {
                        var pair = _c.value;
                        transformedHeaders[pair[0]] = pair[1];
                    }
                }
                catch (e_1_1) { e_1 = { error: e_1_1 }; }
                finally {
                    try {
                        if (_c && !_c.done && (_a = _b.return)) _a.call(_b);
                    }
                    finally { if (e_1) throw e_1.error; }
                }
                var hasReadableStream = response.body !== undefined;
                // Return the response with buffered body
                if (!hasReadableStream) {
                    return response.blob().then(function (body) { return ({
                        response: new HttpResponse({
                            headers: transformedHeaders,
                            statusCode: response.status,
                            body: body,
                        }),
                    }); });
                }
                // Return the response with streaming body
                return {
                    response: new HttpResponse({
                        headers: transformedHeaders,
                        statusCode: response.status,
                        body: response.body,
                    }),
                };
            }),
            requestTimeout(requestTimeoutInMs),
        ];
        if (abortSignal) {
            raceOfPromises.push(new Promise(function (resolve, reject) {
                abortSignal.onabort = function () {
                    var abortError = new Error("Request aborted");
                    abortError.name = "AbortError";
                    reject(abortError);
                };
            }));
        }
        return Promise.race(raceOfPromises);
    };
    return FetchHttpHandler;
}());

var alphabetByEncoding = {};
var alphabetByValue = new Array(64);
for (var i$1 = 0, start = "A".charCodeAt(0), limit = "Z".charCodeAt(0); i$1 + start <= limit; i$1++) {
    var char = String.fromCharCode(i$1 + start);
    alphabetByEncoding[char] = i$1;
    alphabetByValue[i$1] = char;
}
for (var i$1 = 0, start = "a".charCodeAt(0), limit = "z".charCodeAt(0); i$1 + start <= limit; i$1++) {
    var char = String.fromCharCode(i$1 + start);
    var index = i$1 + 26;
    alphabetByEncoding[char] = index;
    alphabetByValue[index] = char;
}
for (var i$1 = 0; i$1 < 10; i$1++) {
    alphabetByEncoding[i$1.toString(10)] = i$1 + 52;
    var char = i$1.toString(10);
    var index = i$1 + 52;
    alphabetByEncoding[char] = index;
    alphabetByValue[index] = char;
}
alphabetByEncoding["+"] = 62;
alphabetByValue[62] = "+";
alphabetByEncoding["/"] = 63;
alphabetByValue[63] = "/";
var bitsPerLetter = 6;
var bitsPerByte = 8;
var maxLetterValue = 63;
/**
 * Converts a base-64 encoded string to a Uint8Array of bytes.
 *
 * @param input The base-64 encoded string
 *
 * @see https://tools.ietf.org/html/rfc4648#section-4
 */
function fromBase64(input) {
    var totalByteLength = (input.length / 4) * 3;
    if (input.substr(-2) === "==") {
        totalByteLength -= 2;
    }
    else if (input.substr(-1) === "=") {
        totalByteLength--;
    }
    var out = new ArrayBuffer(totalByteLength);
    var dataView = new DataView(out);
    for (var i = 0; i < input.length; i += 4) {
        var bits = 0;
        var bitLength = 0;
        for (var j = i, limit = i + 3; j <= limit; j++) {
            if (input[j] !== "=") {
                bits |= alphabetByEncoding[input[j]] << ((limit - j) * bitsPerLetter);
                bitLength += bitsPerLetter;
            }
            else {
                bits >>= bitsPerLetter;
            }
        }
        var chunkOffset = (i / 4) * 3;
        bits >>= bitLength % bitsPerByte;
        var byteLength = Math.floor(bitLength / bitsPerByte);
        for (var k = 0; k < byteLength; k++) {
            var offset = (byteLength - k - 1) * bitsPerByte;
            dataView.setUint8(chunkOffset + k, (bits & (255 << offset)) >> offset);
        }
    }
    return new Uint8Array(out);
}
/**
 * Converts a Uint8Array of binary data to a base-64 encoded string.
 *
 * @param input The binary data to encode
 *
 * @see https://tools.ietf.org/html/rfc4648#section-4
 */
function toBase64(input) {
    var str = "";
    for (var i = 0; i < input.length; i += 3) {
        var bits = 0;
        var bitLength = 0;
        for (var j = i, limit = Math.min(i + 3, input.length); j < limit; j++) {
            bits |= input[j] << ((limit - j - 1) * bitsPerByte);
            bitLength += bitsPerByte;
        }
        var bitClusterCount = Math.ceil(bitLength / bitsPerLetter);
        bits <<= bitClusterCount * bitsPerLetter - bitLength;
        for (var k = 1; k <= bitClusterCount; k++) {
            var offset = (bitClusterCount - k) * bitsPerLetter;
            str += alphabetByValue[(bits & (maxLetterValue << offset)) >> offset];
        }
        str += "==".slice(0, 4 - bitClusterCount);
    }
    return str;
}

//reference: https://snack.expo.io/r1JCSWRGU
var streamCollector = function (stream) {
    if (typeof Blob === "function" && stream instanceof Blob) {
        return collectBlob(stream);
    }
    return collectStream(stream);
};
function collectBlob(blob) {
    return __awaiter$7(this, void 0, void 0, function () {
        var base64, arrayBuffer;
        return __generator$7(this, function (_a) {
            switch (_a.label) {
                case 0: return [4 /*yield*/, readToBase64(blob)];
                case 1:
                    base64 = _a.sent();
                    arrayBuffer = fromBase64(base64);
                    return [2 /*return*/, new Uint8Array(arrayBuffer)];
            }
        });
    });
}
function collectStream(stream) {
    return __awaiter$7(this, void 0, void 0, function () {
        var res, reader, isDone, _a, done, value, prior;
        return __generator$7(this, function (_b) {
            switch (_b.label) {
                case 0:
                    res = new Uint8Array(0);
                    reader = stream.getReader();
                    isDone = false;
                    _b.label = 1;
                case 1:
                    if (!!isDone) return [3 /*break*/, 3];
                    return [4 /*yield*/, reader.read()];
                case 2:
                    _a = _b.sent(), done = _a.done, value = _a.value;
                    if (value) {
                        prior = res;
                        res = new Uint8Array(prior.length + value.length);
                        res.set(prior);
                        res.set(value, prior.length);
                    }
                    isDone = done;
                    return [3 /*break*/, 1];
                case 3: return [2 /*return*/, res];
            }
        });
    });
}
function readToBase64(blob) {
    return new Promise(function (resolve, reject) {
        var reader = new FileReader();
        reader.onloadend = function () {
            var _a;
            // reference: https://developer.mozilla.org/en-US/docs/Web/API/FileReader/readAsDataURL
            // response from readAsDataURL is always prepended with "data:*/*;base64,"
            if (reader.readyState !== 2) {
                return reject(new Error("Reader aborted too early"));
            }
            var result = ((_a = reader.result) !== null && _a !== void 0 ? _a : "");
            // Response can include only 'data:' for empty blob, return empty string in this case.
            // Otherwise, return the string after ','
            var commaIndex = result.indexOf(",");
            var dataOffset = commaIndex > -1 ? commaIndex + 1 : result.length;
            resolve(result.substring(dataOffset));
        };
        reader.onabort = function () { return reject(new Error("Read aborted")); };
        reader.onerror = function () { return reject(reader.error); };
        // reader.readAsArrayBuffer is not always available
        reader.readAsDataURL(blob);
    });
}

var invalidProvider = function (message) { return function () { return Promise.reject(message); }; };

var retryMiddleware = function (options) { return function (next, context) { return function (args) { return __awaiter$7(void 0, void 0, void 0, function () {
    var _a;
    return __generator$7(this, function (_b) {
        if ((_a = options === null || options === void 0 ? void 0 : options.retryStrategy) === null || _a === void 0 ? void 0 : _a.mode)
            context.userAgent = __spread$1((context.userAgent || []), [["cfg/retry-mode", options.retryStrategy.mode]]);
        return [2 /*return*/, options.retryStrategy.retry(next, args)];
    });
}); }; }; };
var retryMiddlewareOptions = {
    name: "retryMiddleware",
    tags: ["RETRY"],
    step: "finalizeRequest",
    priority: "high",
    override: true,
};
var getRetryPlugin = function (options) { return ({
    applyToStack: function (clientStack) {
        clientStack.add(retryMiddleware(options), retryMiddlewareOptions);
    },
}); };

/**
 * The base number of milliseconds to use in calculating a suitable cool-down
 * time when a retryable error is encountered.
 */
var DEFAULT_RETRY_DELAY_BASE = 100;
/**
 * The maximum amount of time (in milliseconds) that will be used as a delay
 * between retry attempts.
 */
var MAXIMUM_RETRY_DELAY = 20 * 1000;
/**
 * The retry delay base (in milliseconds) to use when a throttling error is
 * encountered.
 */
var THROTTLING_RETRY_DELAY_BASE = 500;
/**
 * Initial number of retry tokens in Retry Quota
 */
var INITIAL_RETRY_TOKENS = 500;
/**
 * The total amount of retry tokens to be decremented from retry token balance.
 */
var RETRY_COST = 5;
/**
 * The total amount of retry tokens to be decremented from retry token balance
 * when a throttling error is encountered.
 */
var TIMEOUT_RETRY_COST = 10;
/**
 * The total amount of retry token to be incremented from retry token balance
 * if an SDK operation invocation succeeds without requiring a retry request.
 */
var NO_RETRY_INCREMENT = 1;
/**
 * Header name for SDK invocation ID
 */
var INVOCATION_ID_HEADER = "amz-sdk-invocation-id";
/**
 * Header name for request retry information.
 */
var REQUEST_HEADER = "amz-sdk-request";

/**
 * Errors encountered when the client clock and server clock cannot agree on the
 * current time.
 *
 * These errors are retryable, assuming the SDK has enabled clock skew
 * correction.
 */
var CLOCK_SKEW_ERROR_CODES = [
    "AuthFailure",
    "InvalidSignatureException",
    "RequestExpired",
    "RequestInTheFuture",
    "RequestTimeTooSkewed",
    "SignatureDoesNotMatch",
];
/**
 * Errors that indicate the SDK is being throttled.
 *
 * These errors are always retryable.
 */
var THROTTLING_ERROR_CODES = [
    "BandwidthLimitExceeded",
    "EC2ThrottledException",
    "LimitExceededException",
    "PriorRequestNotComplete",
    "ProvisionedThroughputExceededException",
    "RequestLimitExceeded",
    "RequestThrottled",
    "RequestThrottledException",
    "SlowDown",
    "ThrottledException",
    "Throttling",
    "ThrottlingException",
    "TooManyRequestsException",
    "TransactionInProgressException",
];
/**
 * Error codes that indicate transient issues
 */
var TRANSIENT_ERROR_CODES = ["AbortError", "TimeoutError", "RequestTimeout", "RequestTimeoutException"];
/**
 * Error codes that indicate transient issues
 */
var TRANSIENT_ERROR_STATUS_CODES = [500, 502, 503, 504];

var isRetryableByTrait = function (error) { return error.$retryable !== undefined; };
var isClockSkewError = function (error) { return CLOCK_SKEW_ERROR_CODES.includes(error.name); };
var isThrottlingError = function (error) {
    var _a, _b;
    return ((_a = error.$metadata) === null || _a === void 0 ? void 0 : _a.httpStatusCode) === 429 ||
        THROTTLING_ERROR_CODES.includes(error.name) ||
        ((_b = error.$retryable) === null || _b === void 0 ? void 0 : _b.throttling) == true;
};
var isTransientError = function (error) {
    var _a;
    return TRANSIENT_ERROR_CODES.includes(error.name) ||
        TRANSIENT_ERROR_STATUS_CODES.includes(((_a = error.$metadata) === null || _a === void 0 ? void 0 : _a.httpStatusCode) || 0);
};

var rngBrowser = createCommonjsModule(function (module) {
// Unique ID creation requires a high quality random # generator.  In the
// browser this is a little complicated due to unknown quality of Math.random()
// and inconsistent support for the `crypto` API.  We do the best we can via
// feature-detection

// getRandomValues needs to be invoked in a context where "this" is a Crypto
// implementation. Also, find the complete implementation of crypto on IE11.
var getRandomValues = (typeof(crypto) != 'undefined' && crypto.getRandomValues && crypto.getRandomValues.bind(crypto)) ||
                      (typeof(msCrypto) != 'undefined' && typeof window.msCrypto.getRandomValues == 'function' && msCrypto.getRandomValues.bind(msCrypto));

if (getRandomValues) {
  // WHATWG crypto RNG - http://wiki.whatwg.org/wiki/Crypto
  var rnds8 = new Uint8Array(16); // eslint-disable-line no-undef

  module.exports = function whatwgRNG() {
    getRandomValues(rnds8);
    return rnds8;
  };
} else {
  // Math.random()-based (RNG)
  //
  // If all else fails, use Math.random().  It's fast, but is of unspecified
  // quality.
  var rnds = new Array(16);

  module.exports = function mathRNG() {
    for (var i = 0, r; i < 16; i++) {
      if ((i & 0x03) === 0) r = Math.random() * 0x100000000;
      rnds[i] = r >>> ((i & 0x03) << 3) & 0xff;
    }

    return rnds;
  };
}
});

/**
 * Convert array of 16 byte values to UUID string format of the form:
 * XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX
 */
var byteToHex = [];
for (var i = 0; i < 256; ++i) {
  byteToHex[i] = (i + 0x100).toString(16).substr(1);
}

function bytesToUuid(buf, offset) {
  var i = offset || 0;
  var bth = byteToHex;
  // join used to fix memory issue caused by concatenation: https://bugs.chromium.org/p/v8/issues/detail?id=3175#c4
  return ([
    bth[buf[i++]], bth[buf[i++]],
    bth[buf[i++]], bth[buf[i++]], '-',
    bth[buf[i++]], bth[buf[i++]], '-',
    bth[buf[i++]], bth[buf[i++]], '-',
    bth[buf[i++]], bth[buf[i++]], '-',
    bth[buf[i++]], bth[buf[i++]],
    bth[buf[i++]], bth[buf[i++]],
    bth[buf[i++]], bth[buf[i++]]
  ]).join('');
}

var bytesToUuid_1 = bytesToUuid;

// **`v1()` - Generate time-based UUID**
//
// Inspired by https://github.com/LiosK/UUID.js
// and http://docs.python.org/library/uuid.html

var _nodeId;
var _clockseq;

// Previous uuid creation time
var _lastMSecs = 0;
var _lastNSecs = 0;

// See https://github.com/uuidjs/uuid for API details
function v1(options, buf, offset) {
  var i = buf && offset || 0;
  var b = buf || [];

  options = options || {};
  var node = options.node || _nodeId;
  var clockseq = options.clockseq !== undefined ? options.clockseq : _clockseq;

  // node and clockseq need to be initialized to random values if they're not
  // specified.  We do this lazily to minimize issues related to insufficient
  // system entropy.  See #189
  if (node == null || clockseq == null) {
    var seedBytes = rngBrowser();
    if (node == null) {
      // Per 4.5, create and 48-bit node id, (47 random bits + multicast bit = 1)
      node = _nodeId = [
        seedBytes[0] | 0x01,
        seedBytes[1], seedBytes[2], seedBytes[3], seedBytes[4], seedBytes[5]
      ];
    }
    if (clockseq == null) {
      // Per 4.2.2, randomize (14 bit) clockseq
      clockseq = _clockseq = (seedBytes[6] << 8 | seedBytes[7]) & 0x3fff;
    }
  }

  // UUID timestamps are 100 nano-second units since the Gregorian epoch,
  // (1582-10-15 00:00).  JSNumbers aren't precise enough for this, so
  // time is handled internally as 'msecs' (integer milliseconds) and 'nsecs'
  // (100-nanoseconds offset from msecs) since unix epoch, 1970-01-01 00:00.
  var msecs = options.msecs !== undefined ? options.msecs : new Date().getTime();

  // Per 4.2.1.2, use count of uuid's generated during the current clock
  // cycle to simulate higher resolution clock
  var nsecs = options.nsecs !== undefined ? options.nsecs : _lastNSecs + 1;

  // Time since last uuid creation (in msecs)
  var dt = (msecs - _lastMSecs) + (nsecs - _lastNSecs)/10000;

  // Per 4.2.1.2, Bump clockseq on clock regression
  if (dt < 0 && options.clockseq === undefined) {
    clockseq = clockseq + 1 & 0x3fff;
  }

  // Reset nsecs if clock regresses (new clockseq) or we've moved onto a new
  // time interval
  if ((dt < 0 || msecs > _lastMSecs) && options.nsecs === undefined) {
    nsecs = 0;
  }

  // Per 4.2.1.2 Throw error if too many uuids are requested
  if (nsecs >= 10000) {
    throw new Error('uuid.v1(): Can\'t create more than 10M uuids/sec');
  }

  _lastMSecs = msecs;
  _lastNSecs = nsecs;
  _clockseq = clockseq;

  // Per 4.1.4 - Convert from unix epoch to Gregorian epoch
  msecs += 12219292800000;

  // `time_low`
  var tl = ((msecs & 0xfffffff) * 10000 + nsecs) % 0x100000000;
  b[i++] = tl >>> 24 & 0xff;
  b[i++] = tl >>> 16 & 0xff;
  b[i++] = tl >>> 8 & 0xff;
  b[i++] = tl & 0xff;

  // `time_mid`
  var tmh = (msecs / 0x100000000 * 10000) & 0xfffffff;
  b[i++] = tmh >>> 8 & 0xff;
  b[i++] = tmh & 0xff;

  // `time_high_and_version`
  b[i++] = tmh >>> 24 & 0xf | 0x10; // include version
  b[i++] = tmh >>> 16 & 0xff;

  // `clock_seq_hi_and_reserved` (Per 4.2.2 - include variant)
  b[i++] = clockseq >>> 8 | 0x80;

  // `clock_seq_low`
  b[i++] = clockseq & 0xff;

  // `node`
  for (var n = 0; n < 6; ++n) {
    b[i + n] = node[n];
  }

  return buf ? buf : bytesToUuid_1(b);
}

var v1_1 = v1;

function v4(options, buf, offset) {
  var i = buf && offset || 0;

  if (typeof(options) == 'string') {
    buf = options === 'binary' ? new Array(16) : null;
    options = null;
  }
  options = options || {};

  var rnds = options.random || (options.rng || rngBrowser)();

  // Per 4.4, set bits for version and `clock_seq_hi_and_reserved`
  rnds[6] = (rnds[6] & 0x0f) | 0x40;
  rnds[8] = (rnds[8] & 0x3f) | 0x80;

  // Copy bytes to buffer, if provided
  if (buf) {
    for (var ii = 0; ii < 16; ++ii) {
      buf[i + ii] = rnds[ii];
    }
  }

  return buf || bytesToUuid_1(rnds);
}

var v4_1 = v4;

var uuid = v4_1;
uuid.v1 = v1_1;
uuid.v4 = v4_1;

var uuid_1 = uuid;

var getDefaultRetryQuota = function (initialRetryTokens) {
    var MAX_CAPACITY = initialRetryTokens;
    var availableCapacity = initialRetryTokens;
    var getCapacityAmount = function (error) { return (error.name === "TimeoutError" ? TIMEOUT_RETRY_COST : RETRY_COST); };
    var hasRetryTokens = function (error) { return getCapacityAmount(error) <= availableCapacity; };
    var retrieveRetryTokens = function (error) {
        if (!hasRetryTokens(error)) {
            // retryStrategy should stop retrying, and return last error
            throw new Error("No retry token available");
        }
        var capacityAmount = getCapacityAmount(error);
        availableCapacity -= capacityAmount;
        return capacityAmount;
    };
    var releaseRetryTokens = function (capacityReleaseAmount) {
        availableCapacity += capacityReleaseAmount !== null && capacityReleaseAmount !== void 0 ? capacityReleaseAmount : NO_RETRY_INCREMENT;
        availableCapacity = Math.min(availableCapacity, MAX_CAPACITY);
    };
    return Object.freeze({
        hasRetryTokens: hasRetryTokens,
        retrieveRetryTokens: retrieveRetryTokens,
        releaseRetryTokens: releaseRetryTokens,
    });
};

/**
 * Calculate a capped, fully-jittered exponential backoff time.
 */
var defaultDelayDecider = function (delayBase, attempts) {
    return Math.floor(Math.min(MAXIMUM_RETRY_DELAY, Math.random() * Math.pow(2, attempts) * delayBase));
};

var defaultRetryDecider = function (error) {
    if (!error) {
        return false;
    }
    return isRetryableByTrait(error) || isClockSkewError(error) || isThrottlingError(error) || isTransientError(error);
};

/**
 * The default value for how many HTTP requests an SDK should make for a
 * single SDK operation invocation before giving up
 */
var DEFAULT_MAX_ATTEMPTS = 3;
/**
 * The default retry algorithm to use.
 */
var DEFAULT_RETRY_MODE = "standard";
var StandardRetryStrategy = /** @class */ (function () {
    function StandardRetryStrategy(maxAttemptsProvider, options) {
        var _a, _b, _c;
        this.maxAttemptsProvider = maxAttemptsProvider;
        this.mode = DEFAULT_RETRY_MODE;
        this.retryDecider = (_a = options === null || options === void 0 ? void 0 : options.retryDecider) !== null && _a !== void 0 ? _a : defaultRetryDecider;
        this.delayDecider = (_b = options === null || options === void 0 ? void 0 : options.delayDecider) !== null && _b !== void 0 ? _b : defaultDelayDecider;
        this.retryQuota = (_c = options === null || options === void 0 ? void 0 : options.retryQuota) !== null && _c !== void 0 ? _c : getDefaultRetryQuota(INITIAL_RETRY_TOKENS);
    }
    StandardRetryStrategy.prototype.shouldRetry = function (error, attempts, maxAttempts) {
        return attempts < maxAttempts && this.retryDecider(error) && this.retryQuota.hasRetryTokens(error);
    };
    StandardRetryStrategy.prototype.getMaxAttempts = function () {
        return __awaiter$7(this, void 0, void 0, function () {
            var maxAttempts;
            return __generator$7(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        _a.trys.push([0, 2, , 3]);
                        return [4 /*yield*/, this.maxAttemptsProvider()];
                    case 1:
                        maxAttempts = _a.sent();
                        return [3 /*break*/, 3];
                    case 2:
                        _a.sent();
                        maxAttempts = DEFAULT_MAX_ATTEMPTS;
                        return [3 /*break*/, 3];
                    case 3: return [2 /*return*/, maxAttempts];
                }
            });
        });
    };
    StandardRetryStrategy.prototype.retry = function (next, args) {
        return __awaiter$7(this, void 0, void 0, function () {
            var retryTokenAmount, attempts, totalDelay, maxAttempts, request, _loop_1, this_1, state_1;
            return __generator$7(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        attempts = 0;
                        totalDelay = 0;
                        return [4 /*yield*/, this.getMaxAttempts()];
                    case 1:
                        maxAttempts = _a.sent();
                        request = args.request;
                        if (HttpRequest.isInstance(request)) {
                            request.headers[INVOCATION_ID_HEADER] = uuid_1.v4();
                        }
                        _loop_1 = function () {
                            var _a, response, output, err_1, delay_1;
                            return __generator$7(this, function (_b) {
                                switch (_b.label) {
                                    case 0:
                                        _b.trys.push([0, 2, , 5]);
                                        if (HttpRequest.isInstance(request)) {
                                            request.headers[REQUEST_HEADER] = "attempt=" + (attempts + 1) + "; max=" + maxAttempts;
                                        }
                                        return [4 /*yield*/, next(args)];
                                    case 1:
                                        _a = _b.sent(), response = _a.response, output = _a.output;
                                        this_1.retryQuota.releaseRetryTokens(retryTokenAmount);
                                        output.$metadata.attempts = attempts + 1;
                                        output.$metadata.totalRetryDelay = totalDelay;
                                        return [2 /*return*/, { value: { response: response, output: output } }];
                                    case 2:
                                        err_1 = _b.sent();
                                        attempts++;
                                        if (!this_1.shouldRetry(err_1, attempts, maxAttempts)) return [3 /*break*/, 4];
                                        retryTokenAmount = this_1.retryQuota.retrieveRetryTokens(err_1);
                                        delay_1 = this_1.delayDecider(isThrottlingError(err_1) ? THROTTLING_RETRY_DELAY_BASE : DEFAULT_RETRY_DELAY_BASE, attempts);
                                        totalDelay += delay_1;
                                        return [4 /*yield*/, new Promise(function (resolve) { return setTimeout(resolve, delay_1); })];
                                    case 3:
                                        _b.sent();
                                        return [2 /*return*/, "continue"];
                                    case 4:
                                        if (!err_1.$metadata) {
                                            err_1.$metadata = {};
                                        }
                                        err_1.$metadata.attempts = attempts;
                                        err_1.$metadata.totalRetryDelay = totalDelay;
                                        throw err_1;
                                    case 5: return [2 /*return*/];
                                }
                            });
                        };
                        this_1 = this;
                        _a.label = 2;
                    case 2:
                        return [5 /*yield**/, _loop_1()];
                    case 3:
                        state_1 = _a.sent();
                        if (typeof state_1 === "object")
                            return [2 /*return*/, state_1.value];
                        return [3 /*break*/, 2];
                    case 4: return [2 /*return*/];
                }
            });
        });
    };
    return StandardRetryStrategy;
}());

var resolveRetryConfig = function (input) {
    var maxAttempts = normalizeMaxAttempts(input.maxAttempts);
    return __assign$6(__assign$6({}, input), { maxAttempts: maxAttempts, retryStrategy: input.retryStrategy || new StandardRetryStrategy(maxAttempts) });
};
var normalizeMaxAttempts = function (maxAttempts) {
    if (maxAttempts === void 0) { maxAttempts = DEFAULT_MAX_ATTEMPTS; }
    if (typeof maxAttempts === "number") {
        var promisified_1 = Promise.resolve(maxAttempts);
        return function () { return promisified_1; };
    }
    return maxAttempts;
};

function calculateBodyLength(body) {
    if (typeof body === "string") {
        var len = body.length;
        for (var i = len - 1; i >= 0; i--) {
            var code = body.charCodeAt(i);
            if (code > 0x7f && code <= 0x7ff)
                len++;
            else if (code > 0x7ff && code <= 0xffff)
                len += 2;
        }
        return len;
    }
    else if (typeof body.byteLength === "number") {
        // handles Uint8Array, ArrayBuffer, Buffer, and ArrayBufferView
        return body.byteLength;
    }
    else if (typeof body.size === "number") {
        // handles browser File object
        return body.size;
    }
}

// NOTE: this list must be up-to-date with browsers listed in
// test/acceptance/useragentstrings.yml
const BROWSER_ALIASES_MAP = {
  'Amazon Silk': 'amazon_silk',
  'Android Browser': 'android',
  Bada: 'bada',
  BlackBerry: 'blackberry',
  Chrome: 'chrome',
  Chromium: 'chromium',
  Electron: 'electron',
  Epiphany: 'epiphany',
  Firefox: 'firefox',
  Focus: 'focus',
  Generic: 'generic',
  'Google Search': 'google_search',
  Googlebot: 'googlebot',
  'Internet Explorer': 'ie',
  'K-Meleon': 'k_meleon',
  Maxthon: 'maxthon',
  'Microsoft Edge': 'edge',
  'MZ Browser': 'mz',
  'NAVER Whale Browser': 'naver',
  Opera: 'opera',
  'Opera Coast': 'opera_coast',
  PhantomJS: 'phantomjs',
  Puffin: 'puffin',
  QupZilla: 'qupzilla',
  QQ: 'qq',
  QQLite: 'qqlite',
  Safari: 'safari',
  Sailfish: 'sailfish',
  'Samsung Internet for Android': 'samsung_internet',
  SeaMonkey: 'seamonkey',
  Sleipnir: 'sleipnir',
  Swing: 'swing',
  Tizen: 'tizen',
  'UC Browser': 'uc',
  Vivaldi: 'vivaldi',
  'WebOS Browser': 'webos',
  WeChat: 'wechat',
  'Yandex Browser': 'yandex',
  Roku: 'roku',
};

const BROWSER_MAP = {
  amazon_silk: 'Amazon Silk',
  android: 'Android Browser',
  bada: 'Bada',
  blackberry: 'BlackBerry',
  chrome: 'Chrome',
  chromium: 'Chromium',
  electron: 'Electron',
  epiphany: 'Epiphany',
  firefox: 'Firefox',
  focus: 'Focus',
  generic: 'Generic',
  googlebot: 'Googlebot',
  google_search: 'Google Search',
  ie: 'Internet Explorer',
  k_meleon: 'K-Meleon',
  maxthon: 'Maxthon',
  edge: 'Microsoft Edge',
  mz: 'MZ Browser',
  naver: 'NAVER Whale Browser',
  opera: 'Opera',
  opera_coast: 'Opera Coast',
  phantomjs: 'PhantomJS',
  puffin: 'Puffin',
  qupzilla: 'QupZilla',
  qq: 'QQ Browser',
  qqlite: 'QQ Browser Lite',
  safari: 'Safari',
  sailfish: 'Sailfish',
  samsung_internet: 'Samsung Internet for Android',
  seamonkey: 'SeaMonkey',
  sleipnir: 'Sleipnir',
  swing: 'Swing',
  tizen: 'Tizen',
  uc: 'UC Browser',
  vivaldi: 'Vivaldi',
  webos: 'WebOS Browser',
  wechat: 'WeChat',
  yandex: 'Yandex Browser',
};

const PLATFORMS_MAP = {
  tablet: 'tablet',
  mobile: 'mobile',
  desktop: 'desktop',
  tv: 'tv',
};

const OS_MAP = {
  WindowsPhone: 'Windows Phone',
  Windows: 'Windows',
  MacOS: 'macOS',
  iOS: 'iOS',
  Android: 'Android',
  WebOS: 'WebOS',
  BlackBerry: 'BlackBerry',
  Bada: 'Bada',
  Tizen: 'Tizen',
  Linux: 'Linux',
  ChromeOS: 'Chrome OS',
  PlayStation4: 'PlayStation 4',
  Roku: 'Roku',
};

const ENGINE_MAP = {
  EdgeHTML: 'EdgeHTML',
  Blink: 'Blink',
  Trident: 'Trident',
  Presto: 'Presto',
  Gecko: 'Gecko',
  WebKit: 'WebKit',
};

class Utils {
  /**
   * Get first matched item for a string
   * @param {RegExp} regexp
   * @param {String} ua
   * @return {Array|{index: number, input: string}|*|boolean|string}
   */
  static getFirstMatch(regexp, ua) {
    const match = ua.match(regexp);
    return (match && match.length > 0 && match[1]) || '';
  }

  /**
   * Get second matched item for a string
   * @param regexp
   * @param {String} ua
   * @return {Array|{index: number, input: string}|*|boolean|string}
   */
  static getSecondMatch(regexp, ua) {
    const match = ua.match(regexp);
    return (match && match.length > 1 && match[2]) || '';
  }

  /**
   * Match a regexp and return a constant or undefined
   * @param {RegExp} regexp
   * @param {String} ua
   * @param {*} _const Any const that will be returned if regexp matches the string
   * @return {*}
   */
  static matchAndReturnConst(regexp, ua, _const) {
    if (regexp.test(ua)) {
      return _const;
    }
    return void (0);
  }

  static getWindowsVersionName(version) {
    switch (version) {
      case 'NT': return 'NT';
      case 'XP': return 'XP';
      case 'NT 5.0': return '2000';
      case 'NT 5.1': return 'XP';
      case 'NT 5.2': return '2003';
      case 'NT 6.0': return 'Vista';
      case 'NT 6.1': return '7';
      case 'NT 6.2': return '8';
      case 'NT 6.3': return '8.1';
      case 'NT 10.0': return '10';
      default: return undefined;
    }
  }

  /**
   * Get macOS version name
   *    10.5 - Leopard
   *    10.6 - Snow Leopard
   *    10.7 - Lion
   *    10.8 - Mountain Lion
   *    10.9 - Mavericks
   *    10.10 - Yosemite
   *    10.11 - El Capitan
   *    10.12 - Sierra
   *    10.13 - High Sierra
   *    10.14 - Mojave
   *    10.15 - Catalina
   *
   * @example
   *   getMacOSVersionName("10.14") // 'Mojave'
   *
   * @param  {string} version
   * @return {string} versionName
   */
  static getMacOSVersionName(version) {
    const v = version.split('.').splice(0, 2).map(s => parseInt(s, 10) || 0);
    v.push(0);
    if (v[0] !== 10) return undefined;
    switch (v[1]) {
      case 5: return 'Leopard';
      case 6: return 'Snow Leopard';
      case 7: return 'Lion';
      case 8: return 'Mountain Lion';
      case 9: return 'Mavericks';
      case 10: return 'Yosemite';
      case 11: return 'El Capitan';
      case 12: return 'Sierra';
      case 13: return 'High Sierra';
      case 14: return 'Mojave';
      case 15: return 'Catalina';
      default: return undefined;
    }
  }

  /**
   * Get Android version name
   *    1.5 - Cupcake
   *    1.6 - Donut
   *    2.0 - Eclair
   *    2.1 - Eclair
   *    2.2 - Froyo
   *    2.x - Gingerbread
   *    3.x - Honeycomb
   *    4.0 - Ice Cream Sandwich
   *    4.1 - Jelly Bean
   *    4.4 - KitKat
   *    5.x - Lollipop
   *    6.x - Marshmallow
   *    7.x - Nougat
   *    8.x - Oreo
   *    9.x - Pie
   *
   * @example
   *   getAndroidVersionName("7.0") // 'Nougat'
   *
   * @param  {string} version
   * @return {string} versionName
   */
  static getAndroidVersionName(version) {
    const v = version.split('.').splice(0, 2).map(s => parseInt(s, 10) || 0);
    v.push(0);
    if (v[0] === 1 && v[1] < 5) return undefined;
    if (v[0] === 1 && v[1] < 6) return 'Cupcake';
    if (v[0] === 1 && v[1] >= 6) return 'Donut';
    if (v[0] === 2 && v[1] < 2) return 'Eclair';
    if (v[0] === 2 && v[1] === 2) return 'Froyo';
    if (v[0] === 2 && v[1] > 2) return 'Gingerbread';
    if (v[0] === 3) return 'Honeycomb';
    if (v[0] === 4 && v[1] < 1) return 'Ice Cream Sandwich';
    if (v[0] === 4 && v[1] < 4) return 'Jelly Bean';
    if (v[0] === 4 && v[1] >= 4) return 'KitKat';
    if (v[0] === 5) return 'Lollipop';
    if (v[0] === 6) return 'Marshmallow';
    if (v[0] === 7) return 'Nougat';
    if (v[0] === 8) return 'Oreo';
    if (v[0] === 9) return 'Pie';
    return undefined;
  }

  /**
   * Get version precisions count
   *
   * @example
   *   getVersionPrecision("1.10.3") // 3
   *
   * @param  {string} version
   * @return {number}
   */
  static getVersionPrecision(version) {
    return version.split('.').length;
  }

  /**
   * Calculate browser version weight
   *
   * @example
   *   compareVersions('1.10.2.1',  '1.8.2.1.90')    // 1
   *   compareVersions('1.010.2.1', '1.09.2.1.90');  // 1
   *   compareVersions('1.10.2.1',  '1.10.2.1');     // 0
   *   compareVersions('1.10.2.1',  '1.0800.2');     // -1
   *   compareVersions('1.10.2.1',  '1.10',  true);  // 0
   *
   * @param {String} versionA versions versions to compare
   * @param {String} versionB versions versions to compare
   * @param {boolean} [isLoose] enable loose comparison
   * @return {Number} comparison result: -1 when versionA is lower,
   * 1 when versionA is bigger, 0 when both equal
   */
  /* eslint consistent-return: 1 */
  static compareVersions(versionA, versionB, isLoose = false) {
    // 1) get common precision for both versions, for example for "10.0" and "9" it should be 2
    const versionAPrecision = Utils.getVersionPrecision(versionA);
    const versionBPrecision = Utils.getVersionPrecision(versionB);

    let precision = Math.max(versionAPrecision, versionBPrecision);
    let lastPrecision = 0;

    const chunks = Utils.map([versionA, versionB], (version) => {
      const delta = precision - Utils.getVersionPrecision(version);

      // 2) "9" -> "9.0" (for precision = 2)
      const _version = version + new Array(delta + 1).join('.0');

      // 3) "9.0" -> ["000000000"", "000000009"]
      return Utils.map(_version.split('.'), chunk => new Array(20 - chunk.length).join('0') + chunk).reverse();
    });

    // adjust precision for loose comparison
    if (isLoose) {
      lastPrecision = precision - Math.min(versionAPrecision, versionBPrecision);
    }

    // iterate in reverse order by reversed chunks array
    precision -= 1;
    while (precision >= lastPrecision) {
      // 4) compare: "000000009" > "000000010" = false (but "9" > "10" = true)
      if (chunks[0][precision] > chunks[1][precision]) {
        return 1;
      }

      if (chunks[0][precision] === chunks[1][precision]) {
        if (precision === lastPrecision) {
          // all version chunks are same
          return 0;
        }

        precision -= 1;
      } else if (chunks[0][precision] < chunks[1][precision]) {
        return -1;
      }
    }

    return undefined;
  }

  /**
   * Array::map polyfill
   *
   * @param  {Array} arr
   * @param  {Function} iterator
   * @return {Array}
   */
  static map(arr, iterator) {
    const result = [];
    let i;
    if (Array.prototype.map) {
      return Array.prototype.map.call(arr, iterator);
    }
    for (i = 0; i < arr.length; i += 1) {
      result.push(iterator(arr[i]));
    }
    return result;
  }

  /**
   * Array::find polyfill
   *
   * @param  {Array} arr
   * @param  {Function} predicate
   * @return {Array}
   */
  static find(arr, predicate) {
    let i;
    let l;
    if (Array.prototype.find) {
      return Array.prototype.find.call(arr, predicate);
    }
    for (i = 0, l = arr.length; i < l; i += 1) {
      const value = arr[i];
      if (predicate(value, i)) {
        return value;
      }
    }
    return undefined;
  }

  /**
   * Object::assign polyfill
   *
   * @param  {Object} obj
   * @param  {Object} ...objs
   * @return {Object}
   */
  static assign(obj, ...assigners) {
    const result = obj;
    let i;
    let l;
    if (Object.assign) {
      return Object.assign(obj, ...assigners);
    }
    for (i = 0, l = assigners.length; i < l; i += 1) {
      const assigner = assigners[i];
      if (typeof assigner === 'object' && assigner !== null) {
        const keys = Object.keys(assigner);
        keys.forEach((key) => {
          result[key] = assigner[key];
        });
      }
    }
    return obj;
  }

  /**
   * Get short version/alias for a browser name
   *
   * @example
   *   getBrowserAlias('Microsoft Edge') // edge
   *
   * @param  {string} browserName
   * @return {string}
   */
  static getBrowserAlias(browserName) {
    return BROWSER_ALIASES_MAP[browserName];
  }

  /**
   * Get short version/alias for a browser name
   *
   * @example
   *   getBrowserAlias('edge') // Microsoft Edge
   *
   * @param  {string} browserAlias
   * @return {string}
   */
  static getBrowserTypeByAlias(browserAlias) {
    return BROWSER_MAP[browserAlias] || '';
  }
}

/**
 * Browsers' descriptors
 *
 * The idea of descriptors is simple. You should know about them two simple things:
 * 1. Every descriptor has a method or property called `test` and a `describe` method.
 * 2. Order of descriptors is important.
 *
 * More details:
 * 1. Method or property `test` serves as a way to detect whether the UA string
 * matches some certain browser or not. The `describe` method helps to make a result
 * object with params that show some browser-specific things: name, version, etc.
 * 2. Order of descriptors is important because a Parser goes through them one by one
 * in course. For example, if you insert Chrome's descriptor as the first one,
 * more then a half of browsers will be described as Chrome, because they will pass
 * the Chrome descriptor's test.
 *
 * Descriptor's `test` could be a property with an array of RegExps, where every RegExp
 * will be applied to a UA string to test it whether it matches or not.
 * If a descriptor has two or more regexps in the `test` array it tests them one by one
 * with a logical sum operation. Parser stops if it has found any RegExp that matches the UA.
 *
 * Or `test` could be a method. In that case it gets a Parser instance and should
 * return true/false to get the Parser know if this browser descriptor matches the UA or not.
 */

const commonVersionIdentifier = /version\/(\d+(\.?_?\d+)+)/i;

const browsersList = [
  /* Googlebot */
  {
    test: [/googlebot/i],
    describe(ua) {
      const browser = {
        name: 'Googlebot',
      };
      const version = Utils.getFirstMatch(/googlebot\/(\d+(\.\d+))/i, ua) || Utils.getFirstMatch(commonVersionIdentifier, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },

  /* Opera < 13.0 */
  {
    test: [/opera/i],
    describe(ua) {
      const browser = {
        name: 'Opera',
      };
      const version = Utils.getFirstMatch(commonVersionIdentifier, ua) || Utils.getFirstMatch(/(?:opera)[\s/](\d+(\.?_?\d+)+)/i, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },

  /* Opera > 13.0 */
  {
    test: [/opr\/|opios/i],
    describe(ua) {
      const browser = {
        name: 'Opera',
      };
      const version = Utils.getFirstMatch(/(?:opr|opios)[\s/](\S+)/i, ua) || Utils.getFirstMatch(commonVersionIdentifier, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/SamsungBrowser/i],
    describe(ua) {
      const browser = {
        name: 'Samsung Internet for Android',
      };
      const version = Utils.getFirstMatch(commonVersionIdentifier, ua) || Utils.getFirstMatch(/(?:SamsungBrowser)[\s/](\d+(\.?_?\d+)+)/i, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/Whale/i],
    describe(ua) {
      const browser = {
        name: 'NAVER Whale Browser',
      };
      const version = Utils.getFirstMatch(commonVersionIdentifier, ua) || Utils.getFirstMatch(/(?:whale)[\s/](\d+(?:\.\d+)+)/i, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/MZBrowser/i],
    describe(ua) {
      const browser = {
        name: 'MZ Browser',
      };
      const version = Utils.getFirstMatch(/(?:MZBrowser)[\s/](\d+(?:\.\d+)+)/i, ua) || Utils.getFirstMatch(commonVersionIdentifier, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/focus/i],
    describe(ua) {
      const browser = {
        name: 'Focus',
      };
      const version = Utils.getFirstMatch(/(?:focus)[\s/](\d+(?:\.\d+)+)/i, ua) || Utils.getFirstMatch(commonVersionIdentifier, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/swing/i],
    describe(ua) {
      const browser = {
        name: 'Swing',
      };
      const version = Utils.getFirstMatch(/(?:swing)[\s/](\d+(?:\.\d+)+)/i, ua) || Utils.getFirstMatch(commonVersionIdentifier, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/coast/i],
    describe(ua) {
      const browser = {
        name: 'Opera Coast',
      };
      const version = Utils.getFirstMatch(commonVersionIdentifier, ua) || Utils.getFirstMatch(/(?:coast)[\s/](\d+(\.?_?\d+)+)/i, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/opt\/\d+(?:.?_?\d+)+/i],
    describe(ua) {
      const browser = {
        name: 'Opera Touch',
      };
      const version = Utils.getFirstMatch(/(?:opt)[\s/](\d+(\.?_?\d+)+)/i, ua) || Utils.getFirstMatch(commonVersionIdentifier, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/yabrowser/i],
    describe(ua) {
      const browser = {
        name: 'Yandex Browser',
      };
      const version = Utils.getFirstMatch(/(?:yabrowser)[\s/](\d+(\.?_?\d+)+)/i, ua) || Utils.getFirstMatch(commonVersionIdentifier, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/ucbrowser/i],
    describe(ua) {
      const browser = {
        name: 'UC Browser',
      };
      const version = Utils.getFirstMatch(commonVersionIdentifier, ua) || Utils.getFirstMatch(/(?:ucbrowser)[\s/](\d+(\.?_?\d+)+)/i, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/Maxthon|mxios/i],
    describe(ua) {
      const browser = {
        name: 'Maxthon',
      };
      const version = Utils.getFirstMatch(commonVersionIdentifier, ua) || Utils.getFirstMatch(/(?:Maxthon|mxios)[\s/](\d+(\.?_?\d+)+)/i, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/epiphany/i],
    describe(ua) {
      const browser = {
        name: 'Epiphany',
      };
      const version = Utils.getFirstMatch(commonVersionIdentifier, ua) || Utils.getFirstMatch(/(?:epiphany)[\s/](\d+(\.?_?\d+)+)/i, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/puffin/i],
    describe(ua) {
      const browser = {
        name: 'Puffin',
      };
      const version = Utils.getFirstMatch(commonVersionIdentifier, ua) || Utils.getFirstMatch(/(?:puffin)[\s/](\d+(\.?_?\d+)+)/i, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/sleipnir/i],
    describe(ua) {
      const browser = {
        name: 'Sleipnir',
      };
      const version = Utils.getFirstMatch(commonVersionIdentifier, ua) || Utils.getFirstMatch(/(?:sleipnir)[\s/](\d+(\.?_?\d+)+)/i, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/k-meleon/i],
    describe(ua) {
      const browser = {
        name: 'K-Meleon',
      };
      const version = Utils.getFirstMatch(commonVersionIdentifier, ua) || Utils.getFirstMatch(/(?:k-meleon)[\s/](\d+(\.?_?\d+)+)/i, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/micromessenger/i],
    describe(ua) {
      const browser = {
        name: 'WeChat',
      };
      const version = Utils.getFirstMatch(/(?:micromessenger)[\s/](\d+(\.?_?\d+)+)/i, ua) || Utils.getFirstMatch(commonVersionIdentifier, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/qqbrowser/i],
    describe(ua) {
      const browser = {
        name: (/qqbrowserlite/i).test(ua) ? 'QQ Browser Lite' : 'QQ Browser',
      };
      const version = Utils.getFirstMatch(/(?:qqbrowserlite|qqbrowser)[/](\d+(\.?_?\d+)+)/i, ua) || Utils.getFirstMatch(commonVersionIdentifier, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/msie|trident/i],
    describe(ua) {
      const browser = {
        name: 'Internet Explorer',
      };
      const version = Utils.getFirstMatch(/(?:msie |rv:)(\d+(\.?_?\d+)+)/i, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/\sedg\//i],
    describe(ua) {
      const browser = {
        name: 'Microsoft Edge',
      };

      const version = Utils.getFirstMatch(/\sedg\/(\d+(\.?_?\d+)+)/i, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/edg([ea]|ios)/i],
    describe(ua) {
      const browser = {
        name: 'Microsoft Edge',
      };

      const version = Utils.getSecondMatch(/edg([ea]|ios)\/(\d+(\.?_?\d+)+)/i, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/vivaldi/i],
    describe(ua) {
      const browser = {
        name: 'Vivaldi',
      };
      const version = Utils.getFirstMatch(/vivaldi\/(\d+(\.?_?\d+)+)/i, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/seamonkey/i],
    describe(ua) {
      const browser = {
        name: 'SeaMonkey',
      };
      const version = Utils.getFirstMatch(/seamonkey\/(\d+(\.?_?\d+)+)/i, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/sailfish/i],
    describe(ua) {
      const browser = {
        name: 'Sailfish',
      };

      const version = Utils.getFirstMatch(/sailfish\s?browser\/(\d+(\.\d+)?)/i, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/silk/i],
    describe(ua) {
      const browser = {
        name: 'Amazon Silk',
      };
      const version = Utils.getFirstMatch(/silk\/(\d+(\.?_?\d+)+)/i, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/phantom/i],
    describe(ua) {
      const browser = {
        name: 'PhantomJS',
      };
      const version = Utils.getFirstMatch(/phantomjs\/(\d+(\.?_?\d+)+)/i, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/slimerjs/i],
    describe(ua) {
      const browser = {
        name: 'SlimerJS',
      };
      const version = Utils.getFirstMatch(/slimerjs\/(\d+(\.?_?\d+)+)/i, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/blackberry|\bbb\d+/i, /rim\stablet/i],
    describe(ua) {
      const browser = {
        name: 'BlackBerry',
      };
      const version = Utils.getFirstMatch(commonVersionIdentifier, ua) || Utils.getFirstMatch(/blackberry[\d]+\/(\d+(\.?_?\d+)+)/i, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/(web|hpw)[o0]s/i],
    describe(ua) {
      const browser = {
        name: 'WebOS Browser',
      };
      const version = Utils.getFirstMatch(commonVersionIdentifier, ua) || Utils.getFirstMatch(/w(?:eb)?[o0]sbrowser\/(\d+(\.?_?\d+)+)/i, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/bada/i],
    describe(ua) {
      const browser = {
        name: 'Bada',
      };
      const version = Utils.getFirstMatch(/dolfin\/(\d+(\.?_?\d+)+)/i, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/tizen/i],
    describe(ua) {
      const browser = {
        name: 'Tizen',
      };
      const version = Utils.getFirstMatch(/(?:tizen\s?)?browser\/(\d+(\.?_?\d+)+)/i, ua) || Utils.getFirstMatch(commonVersionIdentifier, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/qupzilla/i],
    describe(ua) {
      const browser = {
        name: 'QupZilla',
      };
      const version = Utils.getFirstMatch(/(?:qupzilla)[\s/](\d+(\.?_?\d+)+)/i, ua) || Utils.getFirstMatch(commonVersionIdentifier, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/firefox|iceweasel|fxios/i],
    describe(ua) {
      const browser = {
        name: 'Firefox',
      };
      const version = Utils.getFirstMatch(/(?:firefox|iceweasel|fxios)[\s/](\d+(\.?_?\d+)+)/i, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/electron/i],
    describe(ua) {
      const browser = {
        name: 'Electron',
      };
      const version = Utils.getFirstMatch(/(?:electron)\/(\d+(\.?_?\d+)+)/i, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/MiuiBrowser/i],
    describe(ua) {
      const browser = {
        name: 'Miui',
      };
      const version = Utils.getFirstMatch(/(?:MiuiBrowser)[\s/](\d+(\.?_?\d+)+)/i, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/chromium/i],
    describe(ua) {
      const browser = {
        name: 'Chromium',
      };
      const version = Utils.getFirstMatch(/(?:chromium)[\s/](\d+(\.?_?\d+)+)/i, ua) || Utils.getFirstMatch(commonVersionIdentifier, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/chrome|crios|crmo/i],
    describe(ua) {
      const browser = {
        name: 'Chrome',
      };
      const version = Utils.getFirstMatch(/(?:chrome|crios|crmo)\/(\d+(\.?_?\d+)+)/i, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },
  {
    test: [/GSA/i],
    describe(ua) {
      const browser = {
        name: 'Google Search',
      };
      const version = Utils.getFirstMatch(/(?:GSA)\/(\d+(\.?_?\d+)+)/i, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },

  /* Android Browser */
  {
    test(parser) {
      const notLikeAndroid = !parser.test(/like android/i);
      const butAndroid = parser.test(/android/i);
      return notLikeAndroid && butAndroid;
    },
    describe(ua) {
      const browser = {
        name: 'Android Browser',
      };
      const version = Utils.getFirstMatch(commonVersionIdentifier, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },

  /* PlayStation 4 */
  {
    test: [/playstation 4/i],
    describe(ua) {
      const browser = {
        name: 'PlayStation 4',
      };
      const version = Utils.getFirstMatch(commonVersionIdentifier, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },

  /* Safari */
  {
    test: [/safari|applewebkit/i],
    describe(ua) {
      const browser = {
        name: 'Safari',
      };
      const version = Utils.getFirstMatch(commonVersionIdentifier, ua);

      if (version) {
        browser.version = version;
      }

      return browser;
    },
  },

  /* Something else */
  {
    test: [/.*/i],
    describe(ua) {
      /* Here we try to make sure that there are explicit details about the device
       * in order to decide what regexp exactly we want to apply
       * (as there is a specific decision based on that conclusion)
       */
      const regexpWithoutDeviceSpec = /^(.*)\/(.*) /;
      const regexpWithDeviceSpec = /^(.*)\/(.*)[ \t]\((.*)/;
      const hasDeviceSpec = ua.search('\\(') !== -1;
      const regexp = hasDeviceSpec ? regexpWithDeviceSpec : regexpWithoutDeviceSpec;
      return {
        name: Utils.getFirstMatch(regexp, ua),
        version: Utils.getSecondMatch(regexp, ua),
      };
    },
  },
];

var osParsersList = [
  /* Roku */
  {
    test: [/Roku\/DVP/],
    describe(ua) {
      const version = Utils.getFirstMatch(/Roku\/DVP-(\d+\.\d+)/i, ua);
      return {
        name: OS_MAP.Roku,
        version,
      };
    },
  },

  /* Windows Phone */
  {
    test: [/windows phone/i],
    describe(ua) {
      const version = Utils.getFirstMatch(/windows phone (?:os)?\s?(\d+(\.\d+)*)/i, ua);
      return {
        name: OS_MAP.WindowsPhone,
        version,
      };
    },
  },

  /* Windows */
  {
    test: [/windows /i],
    describe(ua) {
      const version = Utils.getFirstMatch(/Windows ((NT|XP)( \d\d?.\d)?)/i, ua);
      const versionName = Utils.getWindowsVersionName(version);

      return {
        name: OS_MAP.Windows,
        version,
        versionName,
      };
    },
  },

  /* Firefox on iPad */
  {
    test: [/Macintosh(.*?) FxiOS(.*?)\//],
    describe(ua) {
      const result = {
        name: OS_MAP.iOS,
      };
      const version = Utils.getSecondMatch(/(Version\/)(\d[\d.]+)/, ua);
      if (version) {
        result.version = version;
      }
      return result;
    },
  },

  /* macOS */
  {
    test: [/macintosh/i],
    describe(ua) {
      const version = Utils.getFirstMatch(/mac os x (\d+(\.?_?\d+)+)/i, ua).replace(/[_\s]/g, '.');
      const versionName = Utils.getMacOSVersionName(version);

      const os = {
        name: OS_MAP.MacOS,
        version,
      };
      if (versionName) {
        os.versionName = versionName;
      }
      return os;
    },
  },

  /* iOS */
  {
    test: [/(ipod|iphone|ipad)/i],
    describe(ua) {
      const version = Utils.getFirstMatch(/os (\d+([_\s]\d+)*) like mac os x/i, ua).replace(/[_\s]/g, '.');

      return {
        name: OS_MAP.iOS,
        version,
      };
    },
  },

  /* Android */
  {
    test(parser) {
      const notLikeAndroid = !parser.test(/like android/i);
      const butAndroid = parser.test(/android/i);
      return notLikeAndroid && butAndroid;
    },
    describe(ua) {
      const version = Utils.getFirstMatch(/android[\s/-](\d+(\.\d+)*)/i, ua);
      const versionName = Utils.getAndroidVersionName(version);
      const os = {
        name: OS_MAP.Android,
        version,
      };
      if (versionName) {
        os.versionName = versionName;
      }
      return os;
    },
  },

  /* WebOS */
  {
    test: [/(web|hpw)[o0]s/i],
    describe(ua) {
      const version = Utils.getFirstMatch(/(?:web|hpw)[o0]s\/(\d+(\.\d+)*)/i, ua);
      const os = {
        name: OS_MAP.WebOS,
      };

      if (version && version.length) {
        os.version = version;
      }
      return os;
    },
  },

  /* BlackBerry */
  {
    test: [/blackberry|\bbb\d+/i, /rim\stablet/i],
    describe(ua) {
      const version = Utils.getFirstMatch(/rim\stablet\sos\s(\d+(\.\d+)*)/i, ua)
        || Utils.getFirstMatch(/blackberry\d+\/(\d+([_\s]\d+)*)/i, ua)
        || Utils.getFirstMatch(/\bbb(\d+)/i, ua);

      return {
        name: OS_MAP.BlackBerry,
        version,
      };
    },
  },

  /* Bada */
  {
    test: [/bada/i],
    describe(ua) {
      const version = Utils.getFirstMatch(/bada\/(\d+(\.\d+)*)/i, ua);

      return {
        name: OS_MAP.Bada,
        version,
      };
    },
  },

  /* Tizen */
  {
    test: [/tizen/i],
    describe(ua) {
      const version = Utils.getFirstMatch(/tizen[/\s](\d+(\.\d+)*)/i, ua);

      return {
        name: OS_MAP.Tizen,
        version,
      };
    },
  },

  /* Linux */
  {
    test: [/linux/i],
    describe() {
      return {
        name: OS_MAP.Linux,
      };
    },
  },

  /* Chrome OS */
  {
    test: [/CrOS/],
    describe() {
      return {
        name: OS_MAP.ChromeOS,
      };
    },
  },

  /* Playstation 4 */
  {
    test: [/PlayStation 4/],
    describe(ua) {
      const version = Utils.getFirstMatch(/PlayStation 4[/\s](\d+(\.\d+)*)/i, ua);
      return {
        name: OS_MAP.PlayStation4,
        version,
      };
    },
  },
];

/*
 * Tablets go first since usually they have more specific
 * signs to detect.
 */

var platformParsersList = [
  /* Googlebot */
  {
    test: [/googlebot/i],
    describe() {
      return {
        type: 'bot',
        vendor: 'Google',
      };
    },
  },

  /* Huawei */
  {
    test: [/huawei/i],
    describe(ua) {
      const model = Utils.getFirstMatch(/(can-l01)/i, ua) && 'Nova';
      const platform = {
        type: PLATFORMS_MAP.mobile,
        vendor: 'Huawei',
      };
      if (model) {
        platform.model = model;
      }
      return platform;
    },
  },

  /* Nexus Tablet */
  {
    test: [/nexus\s*(?:7|8|9|10).*/i],
    describe() {
      return {
        type: PLATFORMS_MAP.tablet,
        vendor: 'Nexus',
      };
    },
  },

  /* iPad */
  {
    test: [/ipad/i],
    describe() {
      return {
        type: PLATFORMS_MAP.tablet,
        vendor: 'Apple',
        model: 'iPad',
      };
    },
  },

  /* Firefox on iPad */
  {
    test: [/Macintosh(.*?) FxiOS(.*?)\//],
    describe() {
      return {
        type: PLATFORMS_MAP.tablet,
        vendor: 'Apple',
        model: 'iPad',
      };
    },
  },

  /* Amazon Kindle Fire */
  {
    test: [/kftt build/i],
    describe() {
      return {
        type: PLATFORMS_MAP.tablet,
        vendor: 'Amazon',
        model: 'Kindle Fire HD 7',
      };
    },
  },

  /* Another Amazon Tablet with Silk */
  {
    test: [/silk/i],
    describe() {
      return {
        type: PLATFORMS_MAP.tablet,
        vendor: 'Amazon',
      };
    },
  },

  /* Tablet */
  {
    test: [/tablet(?! pc)/i],
    describe() {
      return {
        type: PLATFORMS_MAP.tablet,
      };
    },
  },

  /* iPod/iPhone */
  {
    test(parser) {
      const iDevice = parser.test(/ipod|iphone/i);
      const likeIDevice = parser.test(/like (ipod|iphone)/i);
      return iDevice && !likeIDevice;
    },
    describe(ua) {
      const model = Utils.getFirstMatch(/(ipod|iphone)/i, ua);
      return {
        type: PLATFORMS_MAP.mobile,
        vendor: 'Apple',
        model,
      };
    },
  },

  /* Nexus Mobile */
  {
    test: [/nexus\s*[0-6].*/i, /galaxy nexus/i],
    describe() {
      return {
        type: PLATFORMS_MAP.mobile,
        vendor: 'Nexus',
      };
    },
  },

  /* Mobile */
  {
    test: [/[^-]mobi/i],
    describe() {
      return {
        type: PLATFORMS_MAP.mobile,
      };
    },
  },

  /* BlackBerry */
  {
    test(parser) {
      return parser.getBrowserName(true) === 'blackberry';
    },
    describe() {
      return {
        type: PLATFORMS_MAP.mobile,
        vendor: 'BlackBerry',
      };
    },
  },

  /* Bada */
  {
    test(parser) {
      return parser.getBrowserName(true) === 'bada';
    },
    describe() {
      return {
        type: PLATFORMS_MAP.mobile,
      };
    },
  },

  /* Windows Phone */
  {
    test(parser) {
      return parser.getBrowserName() === 'windows phone';
    },
    describe() {
      return {
        type: PLATFORMS_MAP.mobile,
        vendor: 'Microsoft',
      };
    },
  },

  /* Android Tablet */
  {
    test(parser) {
      const osMajorVersion = Number(String(parser.getOSVersion()).split('.')[0]);
      return parser.getOSName(true) === 'android' && (osMajorVersion >= 3);
    },
    describe() {
      return {
        type: PLATFORMS_MAP.tablet,
      };
    },
  },

  /* Android Mobile */
  {
    test(parser) {
      return parser.getOSName(true) === 'android';
    },
    describe() {
      return {
        type: PLATFORMS_MAP.mobile,
      };
    },
  },

  /* desktop */
  {
    test(parser) {
      return parser.getOSName(true) === 'macos';
    },
    describe() {
      return {
        type: PLATFORMS_MAP.desktop,
        vendor: 'Apple',
      };
    },
  },

  /* Windows */
  {
    test(parser) {
      return parser.getOSName(true) === 'windows';
    },
    describe() {
      return {
        type: PLATFORMS_MAP.desktop,
      };
    },
  },

  /* Linux */
  {
    test(parser) {
      return parser.getOSName(true) === 'linux';
    },
    describe() {
      return {
        type: PLATFORMS_MAP.desktop,
      };
    },
  },

  /* PlayStation 4 */
  {
    test(parser) {
      return parser.getOSName(true) === 'playstation 4';
    },
    describe() {
      return {
        type: PLATFORMS_MAP.tv,
      };
    },
  },

  /* Roku */
  {
    test(parser) {
      return parser.getOSName(true) === 'roku';
    },
    describe() {
      return {
        type: PLATFORMS_MAP.tv,
      };
    },
  },
];

/*
 * More specific goes first
 */
var enginesParsersList = [
  /* EdgeHTML */
  {
    test(parser) {
      return parser.getBrowserName(true) === 'microsoft edge';
    },
    describe(ua) {
      const isBlinkBased = /\sedg\//i.test(ua);

      // return blink if it's blink-based one
      if (isBlinkBased) {
        return {
          name: ENGINE_MAP.Blink,
        };
      }

      // otherwise match the version and return EdgeHTML
      const version = Utils.getFirstMatch(/edge\/(\d+(\.?_?\d+)+)/i, ua);

      return {
        name: ENGINE_MAP.EdgeHTML,
        version,
      };
    },
  },

  /* Trident */
  {
    test: [/trident/i],
    describe(ua) {
      const engine = {
        name: ENGINE_MAP.Trident,
      };

      const version = Utils.getFirstMatch(/trident\/(\d+(\.?_?\d+)+)/i, ua);

      if (version) {
        engine.version = version;
      }

      return engine;
    },
  },

  /* Presto */
  {
    test(parser) {
      return parser.test(/presto/i);
    },
    describe(ua) {
      const engine = {
        name: ENGINE_MAP.Presto,
      };

      const version = Utils.getFirstMatch(/presto\/(\d+(\.?_?\d+)+)/i, ua);

      if (version) {
        engine.version = version;
      }

      return engine;
    },
  },

  /* Gecko */
  {
    test(parser) {
      const isGecko = parser.test(/gecko/i);
      const likeGecko = parser.test(/like gecko/i);
      return isGecko && !likeGecko;
    },
    describe(ua) {
      const engine = {
        name: ENGINE_MAP.Gecko,
      };

      const version = Utils.getFirstMatch(/gecko\/(\d+(\.?_?\d+)+)/i, ua);

      if (version) {
        engine.version = version;
      }

      return engine;
    },
  },

  /* Blink */
  {
    test: [/(apple)?webkit\/537\.36/i],
    describe() {
      return {
        name: ENGINE_MAP.Blink,
      };
    },
  },

  /* WebKit */
  {
    test: [/(apple)?webkit/i],
    describe(ua) {
      const engine = {
        name: ENGINE_MAP.WebKit,
      };

      const version = Utils.getFirstMatch(/webkit\/(\d+(\.?_?\d+)+)/i, ua);

      if (version) {
        engine.version = version;
      }

      return engine;
    },
  },
];

/**
 * The main class that arranges the whole parsing process.
 */
class Parser {
  /**
   * Create instance of Parser
   *
   * @param {String} UA User-Agent string
   * @param {Boolean} [skipParsing=false] parser can skip parsing in purpose of performance
   * improvements if you need to make a more particular parsing
   * like {@link Parser#parseBrowser} or {@link Parser#parsePlatform}
   *
   * @throw {Error} in case of empty UA String
   *
   * @constructor
   */
  constructor(UA, skipParsing = false) {
    if (UA === void (0) || UA === null || UA === '') {
      throw new Error("UserAgent parameter can't be empty");
    }

    this._ua = UA;

    /**
     * @typedef ParsedResult
     * @property {Object} browser
     * @property {String|undefined} [browser.name]
     * Browser name, like `"Chrome"` or `"Internet Explorer"`
     * @property {String|undefined} [browser.version] Browser version as a String `"12.01.45334.10"`
     * @property {Object} os
     * @property {String|undefined} [os.name] OS name, like `"Windows"` or `"macOS"`
     * @property {String|undefined} [os.version] OS version, like `"NT 5.1"` or `"10.11.1"`
     * @property {String|undefined} [os.versionName] OS name, like `"XP"` or `"High Sierra"`
     * @property {Object} platform
     * @property {String|undefined} [platform.type]
     * platform type, can be either `"desktop"`, `"tablet"` or `"mobile"`
     * @property {String|undefined} [platform.vendor] Vendor of the device,
     * like `"Apple"` or `"Samsung"`
     * @property {String|undefined} [platform.model] Device model,
     * like `"iPhone"` or `"Kindle Fire HD 7"`
     * @property {Object} engine
     * @property {String|undefined} [engine.name]
     * Can be any of this: `WebKit`, `Blink`, `Gecko`, `Trident`, `Presto`, `EdgeHTML`
     * @property {String|undefined} [engine.version] String version of the engine
     */
    this.parsedResult = {};

    if (skipParsing !== true) {
      this.parse();
    }
  }

  /**
   * Get UserAgent string of current Parser instance
   * @return {String} User-Agent String of the current <Parser> object
   *
   * @public
   */
  getUA() {
    return this._ua;
  }

  /**
   * Test a UA string for a regexp
   * @param {RegExp} regex
   * @return {Boolean}
   */
  test(regex) {
    return regex.test(this._ua);
  }

  /**
   * Get parsed browser object
   * @return {Object}
   */
  parseBrowser() {
    this.parsedResult.browser = {};

    const browserDescriptor = Utils.find(browsersList, (_browser) => {
      if (typeof _browser.test === 'function') {
        return _browser.test(this);
      }

      if (_browser.test instanceof Array) {
        return _browser.test.some(condition => this.test(condition));
      }

      throw new Error("Browser's test function is not valid");
    });

    if (browserDescriptor) {
      this.parsedResult.browser = browserDescriptor.describe(this.getUA());
    }

    return this.parsedResult.browser;
  }

  /**
   * Get parsed browser object
   * @return {Object}
   *
   * @public
   */
  getBrowser() {
    if (this.parsedResult.browser) {
      return this.parsedResult.browser;
    }

    return this.parseBrowser();
  }

  /**
   * Get browser's name
   * @return {String} Browser's name or an empty string
   *
   * @public
   */
  getBrowserName(toLowerCase) {
    if (toLowerCase) {
      return String(this.getBrowser().name).toLowerCase() || '';
    }
    return this.getBrowser().name || '';
  }


  /**
   * Get browser's version
   * @return {String} version of browser
   *
   * @public
   */
  getBrowserVersion() {
    return this.getBrowser().version;
  }

  /**
   * Get OS
   * @return {Object}
   *
   * @example
   * this.getOS();
   * {
   *   name: 'macOS',
   *   version: '10.11.12'
   * }
   */
  getOS() {
    if (this.parsedResult.os) {
      return this.parsedResult.os;
    }

    return this.parseOS();
  }

  /**
   * Parse OS and save it to this.parsedResult.os
   * @return {*|{}}
   */
  parseOS() {
    this.parsedResult.os = {};

    const os = Utils.find(osParsersList, (_os) => {
      if (typeof _os.test === 'function') {
        return _os.test(this);
      }

      if (_os.test instanceof Array) {
        return _os.test.some(condition => this.test(condition));
      }

      throw new Error("Browser's test function is not valid");
    });

    if (os) {
      this.parsedResult.os = os.describe(this.getUA());
    }

    return this.parsedResult.os;
  }

  /**
   * Get OS name
   * @param {Boolean} [toLowerCase] return lower-cased value
   * @return {String} name of the OS — macOS, Windows, Linux, etc.
   */
  getOSName(toLowerCase) {
    const { name } = this.getOS();

    if (toLowerCase) {
      return String(name).toLowerCase() || '';
    }

    return name || '';
  }

  /**
   * Get OS version
   * @return {String} full version with dots ('10.11.12', '5.6', etc)
   */
  getOSVersion() {
    return this.getOS().version;
  }

  /**
   * Get parsed platform
   * @return {{}}
   */
  getPlatform() {
    if (this.parsedResult.platform) {
      return this.parsedResult.platform;
    }

    return this.parsePlatform();
  }

  /**
   * Get platform name
   * @param {Boolean} [toLowerCase=false]
   * @return {*}
   */
  getPlatformType(toLowerCase = false) {
    const { type } = this.getPlatform();

    if (toLowerCase) {
      return String(type).toLowerCase() || '';
    }

    return type || '';
  }

  /**
   * Get parsed platform
   * @return {{}}
   */
  parsePlatform() {
    this.parsedResult.platform = {};

    const platform = Utils.find(platformParsersList, (_platform) => {
      if (typeof _platform.test === 'function') {
        return _platform.test(this);
      }

      if (_platform.test instanceof Array) {
        return _platform.test.some(condition => this.test(condition));
      }

      throw new Error("Browser's test function is not valid");
    });

    if (platform) {
      this.parsedResult.platform = platform.describe(this.getUA());
    }

    return this.parsedResult.platform;
  }

  /**
   * Get parsed engine
   * @return {{}}
   */
  getEngine() {
    if (this.parsedResult.engine) {
      return this.parsedResult.engine;
    }

    return this.parseEngine();
  }

  /**
   * Get engines's name
   * @return {String} Engines's name or an empty string
   *
   * @public
   */
  getEngineName(toLowerCase) {
    if (toLowerCase) {
      return String(this.getEngine().name).toLowerCase() || '';
    }
    return this.getEngine().name || '';
  }

  /**
   * Get parsed platform
   * @return {{}}
   */
  parseEngine() {
    this.parsedResult.engine = {};

    const engine = Utils.find(enginesParsersList, (_engine) => {
      if (typeof _engine.test === 'function') {
        return _engine.test(this);
      }

      if (_engine.test instanceof Array) {
        return _engine.test.some(condition => this.test(condition));
      }

      throw new Error("Browser's test function is not valid");
    });

    if (engine) {
      this.parsedResult.engine = engine.describe(this.getUA());
    }

    return this.parsedResult.engine;
  }

  /**
   * Parse full information about the browser
   * @returns {Parser}
   */
  parse() {
    this.parseBrowser();
    this.parseOS();
    this.parsePlatform();
    this.parseEngine();

    return this;
  }

  /**
   * Get parsed result
   * @return {ParsedResult}
   */
  getResult() {
    return Utils.assign({}, this.parsedResult);
  }

  /**
   * Check if parsed browser matches certain conditions
   *
   * @param {Object} checkTree It's one or two layered object,
   * which can include a platform or an OS on the first layer
   * and should have browsers specs on the bottom-laying layer
   *
   * @returns {Boolean|undefined} Whether the browser satisfies the set conditions or not.
   * Returns `undefined` when the browser is no described in the checkTree object.
   *
   * @example
   * const browser = Bowser.getParser(window.navigator.userAgent);
   * if (browser.satisfies({chrome: '>118.01.1322' }))
   * // or with os
   * if (browser.satisfies({windows: { chrome: '>118.01.1322' } }))
   * // or with platforms
   * if (browser.satisfies({desktop: { chrome: '>118.01.1322' } }))
   */
  satisfies(checkTree) {
    const platformsAndOSes = {};
    let platformsAndOSCounter = 0;
    const browsers = {};
    let browsersCounter = 0;

    const allDefinitions = Object.keys(checkTree);

    allDefinitions.forEach((key) => {
      const currentDefinition = checkTree[key];
      if (typeof currentDefinition === 'string') {
        browsers[key] = currentDefinition;
        browsersCounter += 1;
      } else if (typeof currentDefinition === 'object') {
        platformsAndOSes[key] = currentDefinition;
        platformsAndOSCounter += 1;
      }
    });

    if (platformsAndOSCounter > 0) {
      const platformsAndOSNames = Object.keys(platformsAndOSes);
      const OSMatchingDefinition = Utils.find(platformsAndOSNames, name => (this.isOS(name)));

      if (OSMatchingDefinition) {
        const osResult = this.satisfies(platformsAndOSes[OSMatchingDefinition]);

        if (osResult !== void 0) {
          return osResult;
        }
      }

      const platformMatchingDefinition = Utils.find(
        platformsAndOSNames,
        name => (this.isPlatform(name)),
      );
      if (platformMatchingDefinition) {
        const platformResult = this.satisfies(platformsAndOSes[platformMatchingDefinition]);

        if (platformResult !== void 0) {
          return platformResult;
        }
      }
    }

    if (browsersCounter > 0) {
      const browserNames = Object.keys(browsers);
      const matchingDefinition = Utils.find(browserNames, name => (this.isBrowser(name, true)));

      if (matchingDefinition !== void 0) {
        return this.compareVersion(browsers[matchingDefinition]);
      }
    }

    return undefined;
  }

  /**
   * Check if the browser name equals the passed string
   * @param browserName The string to compare with the browser name
   * @param [includingAlias=false] The flag showing whether alias will be included into comparison
   * @returns {boolean}
   */
  isBrowser(browserName, includingAlias = false) {
    const defaultBrowserName = this.getBrowserName().toLowerCase();
    let browserNameLower = browserName.toLowerCase();
    const alias = Utils.getBrowserTypeByAlias(browserNameLower);

    if (includingAlias && alias) {
      browserNameLower = alias.toLowerCase();
    }
    return browserNameLower === defaultBrowserName;
  }

  compareVersion(version) {
    let expectedResults = [0];
    let comparableVersion = version;
    let isLoose = false;

    const currentBrowserVersion = this.getBrowserVersion();

    if (typeof currentBrowserVersion !== 'string') {
      return void 0;
    }

    if (version[0] === '>' || version[0] === '<') {
      comparableVersion = version.substr(1);
      if (version[1] === '=') {
        isLoose = true;
        comparableVersion = version.substr(2);
      } else {
        expectedResults = [];
      }
      if (version[0] === '>') {
        expectedResults.push(1);
      } else {
        expectedResults.push(-1);
      }
    } else if (version[0] === '=') {
      comparableVersion = version.substr(1);
    } else if (version[0] === '~') {
      isLoose = true;
      comparableVersion = version.substr(1);
    }

    return expectedResults.indexOf(
      Utils.compareVersions(currentBrowserVersion, comparableVersion, isLoose),
    ) > -1;
  }

  isOS(osName) {
    return this.getOSName(true) === String(osName).toLowerCase();
  }

  isPlatform(platformType) {
    return this.getPlatformType(true) === String(platformType).toLowerCase();
  }

  isEngine(engineName) {
    return this.getEngineName(true) === String(engineName).toLowerCase();
  }

  /**
   * Is anything? Check if the browser is called "anything",
   * the OS called "anything" or the platform called "anything"
   * @param {String} anything
   * @param [includingAlias=false] The flag showing whether alias will be included into comparison
   * @returns {Boolean}
   */
  is(anything, includingAlias = false) {
    return this.isBrowser(anything, includingAlias) || this.isOS(anything)
      || this.isPlatform(anything);
  }

  /**
   * Check if any of the given values satisfies this.is(anything)
   * @param {String[]} anythings
   * @returns {Boolean}
   */
  some(anythings = []) {
    return anythings.some(anything => this.is(anything));
  }
}

/*!
 * Bowser - a browser detector
 * https://github.com/lancedikson/bowser
 * MIT License | (c) Dustin Diaz 2012-2015
 * MIT License | (c) Denis Demchenko 2015-2019
 */

/**
 * Bowser class.
 * Keep it simple as much as it can be.
 * It's supposed to work with collections of {@link Parser} instances
 * rather then solve one-instance problems.
 * All the one-instance stuff is located in Parser class.
 *
 * @class
 * @classdesc Bowser is a static object, that provides an API to the Parsers
 * @hideconstructor
 */
class Bowser {
  /**
   * Creates a {@link Parser} instance
   *
   * @param {String} UA UserAgent string
   * @param {Boolean} [skipParsing=false] Will make the Parser postpone parsing until you ask it
   * explicitly. Same as `skipParsing` for {@link Parser}.
   * @returns {Parser}
   * @throws {Error} when UA is not a String
   *
   * @example
   * const parser = Bowser.getParser(window.navigator.userAgent);
   * const result = parser.getResult();
   */
  static getParser(UA, skipParsing = false) {
    if (typeof UA !== 'string') {
      throw new Error('UserAgent should be a string');
    }
    return new Parser(UA, skipParsing);
  }

  /**
   * Creates a {@link Parser} instance and runs {@link Parser.getResult} immediately
   *
   * @param UA
   * @return {ParsedResult}
   *
   * @example
   * const result = Bowser.parse(window.navigator.userAgent);
   */
  static parse(UA) {
    return (new Parser(UA)).getResult();
  }

  static get BROWSER_MAP() {
    return BROWSER_MAP;
  }

  static get ENGINE_MAP() {
    return ENGINE_MAP;
  }

  static get OS_MAP() {
    return OS_MAP;
  }

  static get PLATFORMS_MAP() {
    return PLATFORMS_MAP;
  }
}

/**
 * Default provider to the user agent in browsers. It's a best effort to infer
 * the device information. It uses bowser library to detect the browser and virsion
 */
var defaultUserAgent = function (_a) {
    var serviceId = _a.serviceId, clientVersion = _a.clientVersion;
    return function () { return __awaiter$7(void 0, void 0, void 0, function () {
        var parsedUA, sections;
        var _a, _b, _c, _d, _e, _f, _g;
        return __generator$7(this, function (_h) {
            parsedUA = ((_a = window === null || window === void 0 ? void 0 : window.navigator) === null || _a === void 0 ? void 0 : _a.userAgent) ? Bowser.parse(window.navigator.userAgent) : undefined;
            sections = [
                // sdk-metadata
                ["aws-sdk-js", clientVersion],
                // os-metadata
                ["os/" + (((_b = parsedUA === null || parsedUA === void 0 ? void 0 : parsedUA.os) === null || _b === void 0 ? void 0 : _b.name) || "other"), (_c = parsedUA === null || parsedUA === void 0 ? void 0 : parsedUA.os) === null || _c === void 0 ? void 0 : _c.version],
                // language-metadata
                // ECMAScript edition doesn't matter in JS.
                ["lang/js"],
                // browser vendor and version.
                ["md/browser", ((_e = (_d = parsedUA === null || parsedUA === void 0 ? void 0 : parsedUA.browser) === null || _d === void 0 ? void 0 : _d.name) !== null && _e !== void 0 ? _e : "unknown") + "_" + ((_g = (_f = parsedUA === null || parsedUA === void 0 ? void 0 : parsedUA.browser) === null || _f === void 0 ? void 0 : _f.version) !== null && _g !== void 0 ? _g : "unknown")],
            ];
            if (serviceId) {
                // api-metadata
                // service Id may not appear in non-AWS clients
                sections.push(["api/" + serviceId, clientVersion]);
            }
            return [2 /*return*/, sections];
        });
    }); };
};

function parseQueryString(querystring) {
    var e_1, _a;
    var query = {};
    querystring = querystring.replace(/^\?/, "");
    if (querystring) {
        try {
            for (var _b = __values(querystring.split("&")), _c = _b.next(); !_c.done; _c = _b.next()) {
                var pair = _c.value;
                var _d = __read$4(pair.split("="), 2), key = _d[0], _e = _d[1], value = _e === void 0 ? null : _e;
                key = decodeURIComponent(key);
                if (value) {
                    value = decodeURIComponent(value);
                }
                if (!(key in query)) {
                    query[key] = value;
                }
                else if (Array.isArray(query[key])) {
                    query[key].push(value);
                }
                else {
                    query[key] = [query[key], value];
                }
            }
        }
        catch (e_1_1) { e_1 = { error: e_1_1 }; }
        finally {
            try {
                if (_c && !_c.done && (_a = _b.return)) _a.call(_b);
            }
            finally { if (e_1) throw e_1.error; }
        }
    }
    return query;
}

var parseUrl = function (url) {
    var _a = new URL(url), hostname = _a.hostname, pathname = _a.pathname, port = _a.port, protocol = _a.protocol, search = _a.search;
    var query;
    if (search) {
        query = parseQueryString(search);
    }
    return {
        hostname: hostname,
        port: port ? parseInt(port) : undefined,
        protocol: protocol,
        path: pathname,
        query: query,
    };
};

var resolveEndpointsConfig = function (input) {
    var _a;
    return (__assign$6(__assign$6({}, input), { tls: (_a = input.tls) !== null && _a !== void 0 ? _a : true, endpoint: input.endpoint ? normalizeEndpoint(input) : function () { return getEndPointFromRegion(input); }, isCustomEndpoint: input.endpoint ? true : false }));
};
var normalizeEndpoint = function (input) {
    var endpoint = input.endpoint, urlParser = input.urlParser;
    if (typeof endpoint === "string") {
        var promisified_1 = Promise.resolve(urlParser(endpoint));
        return function () { return promisified_1; };
    }
    else if (typeof endpoint === "object") {
        var promisified_2 = Promise.resolve(endpoint);
        return function () { return promisified_2; };
    }
    return endpoint;
};
var getEndPointFromRegion = function (input) { return __awaiter$7(void 0, void 0, void 0, function () {
    var _a, tls, region, dnsHostRegex, hostname;
    var _b;
    return __generator$7(this, function (_c) {
        switch (_c.label) {
            case 0:
                _a = input.tls, tls = _a === void 0 ? true : _a;
                return [4 /*yield*/, input.region()];
            case 1:
                region = _c.sent();
                dnsHostRegex = new RegExp(/^([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9-]{0,61}[a-zA-Z0-9])$/);
                if (!dnsHostRegex.test(region)) {
                    throw new Error("Invalid region in client config");
                }
                return [4 /*yield*/, input.regionInfoProvider(region)];
            case 2:
                hostname = ((_b = (_c.sent())) !== null && _b !== void 0 ? _b : {}).hostname;
                if (!hostname) {
                    throw new Error("Cannot resolve hostname from client config");
                }
                return [2 /*return*/, input.urlParser((tls ? "https:" : "http:") + "//" + hostname)];
        }
    });
}); };

var resolveRegionConfig = function (input) {
    if (!input.region) {
        throw new Error("Region is missing");
    }
    return __assign$6(__assign$6({}, input), { region: normalizeRegion(input.region) });
};
var normalizeRegion = function (region) {
    if (typeof region === "string") {
        var promisified_1 = Promise.resolve(region);
        return function () { return promisified_1; };
    }
    return region;
};

var CONTENT_LENGTH_HEADER = "content-length";
function contentLengthMiddleware(bodyLengthChecker) {
    var _this = this;
    return function (next) { return function (args) { return __awaiter$7(_this, void 0, void 0, function () {
        var request, body, headers, length;
        var _a;
        return __generator$7(this, function (_b) {
            request = args.request;
            if (HttpRequest.isInstance(request)) {
                body = request.body, headers = request.headers;
                if (body &&
                    Object.keys(headers)
                        .map(function (str) { return str.toLowerCase(); })
                        .indexOf(CONTENT_LENGTH_HEADER) === -1) {
                    length = bodyLengthChecker(body);
                    if (length !== undefined) {
                        request.headers = __assign$6(__assign$6({}, request.headers), (_a = {}, _a[CONTENT_LENGTH_HEADER] = String(length), _a));
                    }
                }
            }
            return [2 /*return*/, next(__assign$6(__assign$6({}, args), { request: request }))];
        });
    }); }; };
}
var contentLengthMiddlewareOptions = {
    step: "build",
    tags: ["SET_CONTENT_LENGTH", "CONTENT_LENGTH"],
    name: "contentLengthMiddleware",
    override: true,
};
var getContentLengthPlugin = function (options) { return ({
    applyToStack: function (clientStack) {
        clientStack.add(contentLengthMiddleware(options.bodyLengthChecker), contentLengthMiddlewareOptions);
    },
}); };

function resolveHostHeaderConfig(input) {
    return input;
}
var hostHeaderMiddleware = function (options) { return function (next) { return function (args) { return __awaiter$7(void 0, void 0, void 0, function () {
    var request, _a, handlerProtocol;
    return __generator$7(this, function (_b) {
        if (!HttpRequest.isInstance(args.request))
            return [2 /*return*/, next(args)];
        request = args.request;
        _a = (options.requestHandler.metadata || {}).handlerProtocol, handlerProtocol = _a === void 0 ? "" : _a;
        //For H2 request, remove 'host' header and use ':authority' header instead
        //reference: https://nodejs.org/dist/latest-v13.x/docs/api/errors.html#ERR_HTTP2_INVALID_CONNECTION_HEADERS
        if (handlerProtocol.indexOf("h2") >= 0 && !request.headers[":authority"]) {
            delete request.headers["host"];
            request.headers[":authority"] = "";
            //non-H2 request and 'host' header is not set, set the 'host' header to request's hostname.
        }
        else if (!request.headers["host"]) {
            request.headers["host"] = request.hostname;
        }
        return [2 /*return*/, next(args)];
    });
}); }; }; };
var hostHeaderMiddlewareOptions = {
    name: "hostHeaderMiddleware",
    step: "build",
    priority: "low",
    tags: ["HOST"],
    override: true,
};
var getHostHeaderPlugin = function (options) { return ({
    applyToStack: function (clientStack) {
        clientStack.add(hostHeaderMiddleware(options), hostHeaderMiddlewareOptions);
    },
}); };

var loggerMiddleware = function () { return function (next, context) { return function (args) { return __awaiter$7(void 0, void 0, void 0, function () {
    var clientName, commandName, inputFilterSensitiveLog, logger, outputFilterSensitiveLog, response, _a, $metadata, outputWithoutMetadata;
    return __generator$7(this, function (_b) {
        switch (_b.label) {
            case 0:
                clientName = context.clientName, commandName = context.commandName, inputFilterSensitiveLog = context.inputFilterSensitiveLog, logger = context.logger, outputFilterSensitiveLog = context.outputFilterSensitiveLog;
                return [4 /*yield*/, next(args)];
            case 1:
                response = _b.sent();
                if (!logger) {
                    return [2 /*return*/, response];
                }
                if (typeof logger.info === "function") {
                    _a = response.output, $metadata = _a.$metadata, outputWithoutMetadata = __rest(_a, ["$metadata"]);
                    logger.info({
                        clientName: clientName,
                        commandName: commandName,
                        input: inputFilterSensitiveLog(args.input),
                        output: outputFilterSensitiveLog(outputWithoutMetadata),
                        metadata: $metadata,
                    });
                }
                return [2 /*return*/, response];
        }
    });
}); }; }; };
var loggerMiddlewareOptions = {
    name: "loggerMiddleware",
    tags: ["LOGGER"],
    step: "initialize",
    override: true,
};
// eslint-disable-next-line @typescript-eslint/no-unused-vars
var getLoggerPlugin = function (options) { return ({
    applyToStack: function (clientStack) {
        clientStack.add(loggerMiddleware(), loggerMiddlewareOptions);
    },
}); };

var ALGORITHM_QUERY_PARAM = "X-Amz-Algorithm";
var CREDENTIAL_QUERY_PARAM = "X-Amz-Credential";
var AMZ_DATE_QUERY_PARAM = "X-Amz-Date";
var SIGNED_HEADERS_QUERY_PARAM = "X-Amz-SignedHeaders";
var EXPIRES_QUERY_PARAM = "X-Amz-Expires";
var SIGNATURE_QUERY_PARAM = "X-Amz-Signature";
var TOKEN_QUERY_PARAM = "X-Amz-Security-Token";
var AUTH_HEADER = "authorization";
var AMZ_DATE_HEADER = AMZ_DATE_QUERY_PARAM.toLowerCase();
var DATE_HEADER = "date";
var GENERATED_HEADERS = [AUTH_HEADER, AMZ_DATE_HEADER, DATE_HEADER];
var SIGNATURE_HEADER = SIGNATURE_QUERY_PARAM.toLowerCase();
var SHA256_HEADER = "x-amz-content-sha256";
var TOKEN_HEADER = TOKEN_QUERY_PARAM.toLowerCase();
var ALWAYS_UNSIGNABLE_HEADERS = {
    authorization: true,
    "cache-control": true,
    connection: true,
    expect: true,
    from: true,
    "keep-alive": true,
    "max-forwards": true,
    pragma: true,
    referer: true,
    te: true,
    trailer: true,
    "transfer-encoding": true,
    upgrade: true,
    "user-agent": true,
    "x-amzn-trace-id": true,
};
var PROXY_HEADER_PATTERN = /^proxy-/;
var SEC_HEADER_PATTERN = /^sec-/;
var ALGORITHM_IDENTIFIER = "AWS4-HMAC-SHA256";
var EVENT_ALGORITHM_IDENTIFIER = "AWS4-HMAC-SHA256-PAYLOAD";
var UNSIGNED_PAYLOAD = "UNSIGNED-PAYLOAD";
var MAX_CACHE_SIZE = 50;
var KEY_TYPE_IDENTIFIER = "aws4_request";
var MAX_PRESIGNED_TTL = 60 * 60 * 24 * 7;

var signingKeyCache = {};
var cacheQueue = [];
/**
 * Create a string describing the scope of credentials used to sign a request.
 *
 * @param shortDate The current calendar date in the form YYYYMMDD.
 * @param region    The AWS region in which the service resides.
 * @param service   The service to which the signed request is being sent.
 */
function createScope(shortDate, region, service) {
    return shortDate + "/" + region + "/" + service + "/" + KEY_TYPE_IDENTIFIER;
}
/**
 * Derive a signing key from its composite parts
 *
 * @param sha256Constructor A constructor function that can instantiate SHA-256
 *                          hash objects.
 * @param credentials       The credentials with which the request will be
 *                          signed.
 * @param shortDate         The current calendar date in the form YYYYMMDD.
 * @param region            The AWS region in which the service resides.
 * @param service           The service to which the signed request is being
 *                          sent.
 */
var getSigningKey = function (sha256Constructor, credentials, shortDate, region, service) { return __awaiter$7(void 0, void 0, void 0, function () {
    var credsHash, cacheKey, key, _a, _b, signable, e_1_1;
    var e_1, _c;
    return __generator$7(this, function (_d) {
        switch (_d.label) {
            case 0: return [4 /*yield*/, hmac$1(sha256Constructor, credentials.secretAccessKey, credentials.accessKeyId)];
            case 1:
                credsHash = _d.sent();
                cacheKey = shortDate + ":" + region + ":" + service + ":" + toHex$1(credsHash) + ":" + credentials.sessionToken;
                if (cacheKey in signingKeyCache) {
                    return [2 /*return*/, signingKeyCache[cacheKey]];
                }
                cacheQueue.push(cacheKey);
                while (cacheQueue.length > MAX_CACHE_SIZE) {
                    delete signingKeyCache[cacheQueue.shift()];
                }
                key = "AWS4" + credentials.secretAccessKey;
                _d.label = 2;
            case 2:
                _d.trys.push([2, 7, 8, 9]);
                _a = __values([shortDate, region, service, KEY_TYPE_IDENTIFIER]), _b = _a.next();
                _d.label = 3;
            case 3:
                if (!!_b.done) return [3 /*break*/, 6];
                signable = _b.value;
                return [4 /*yield*/, hmac$1(sha256Constructor, key, signable)];
            case 4:
                key = _d.sent();
                _d.label = 5;
            case 5:
                _b = _a.next();
                return [3 /*break*/, 3];
            case 6: return [3 /*break*/, 9];
            case 7:
                e_1_1 = _d.sent();
                e_1 = { error: e_1_1 };
                return [3 /*break*/, 9];
            case 8:
                try {
                    if (_b && !_b.done && (_c = _a.return)) _c.call(_a);
                }
                finally { if (e_1) throw e_1.error; }
                return [7 /*endfinally*/];
            case 9: return [2 /*return*/, (signingKeyCache[cacheKey] = key)];
        }
    });
}); };
function hmac$1(ctor, secret, data) {
    var hash = new ctor(secret);
    hash.update(data);
    return hash.digest();
}

/**
 * @internal
 */
function getCanonicalHeaders(_a, unsignableHeaders, signableHeaders) {
    var e_1, _b;
    var headers = _a.headers;
    var canonical = {};
    try {
        for (var _c = __values(Object.keys(headers).sort()), _d = _c.next(); !_d.done; _d = _c.next()) {
            var headerName = _d.value;
            var canonicalHeaderName = headerName.toLowerCase();
            if (canonicalHeaderName in ALWAYS_UNSIGNABLE_HEADERS || (unsignableHeaders === null || unsignableHeaders === void 0 ? void 0 : unsignableHeaders.has(canonicalHeaderName)) ||
                PROXY_HEADER_PATTERN.test(canonicalHeaderName) ||
                SEC_HEADER_PATTERN.test(canonicalHeaderName)) {
                if (!signableHeaders || (signableHeaders && !signableHeaders.has(canonicalHeaderName))) {
                    continue;
                }
            }
            canonical[canonicalHeaderName] = headers[headerName].trim().replace(/\s+/g, " ");
        }
    }
    catch (e_1_1) { e_1 = { error: e_1_1 }; }
    finally {
        try {
            if (_d && !_d.done && (_b = _c.return)) _b.call(_c);
        }
        finally { if (e_1) throw e_1.error; }
    }
    return canonical;
}

/**
 * @internal
 */
function getCanonicalQuery(_a) {
    var e_1, _b;
    var _c = _a.query, query = _c === void 0 ? {} : _c;
    var keys = [];
    var serialized = {};
    var _loop_1 = function (key) {
        if (key.toLowerCase() === SIGNATURE_HEADER) {
            return "continue";
        }
        keys.push(key);
        var value = query[key];
        if (typeof value === "string") {
            serialized[key] = escapeUri(key) + "=" + escapeUri(value);
        }
        else if (Array.isArray(value)) {
            serialized[key] = value
                .slice(0)
                .sort()
                .reduce(function (encoded, value) { return encoded.concat([escapeUri(key) + "=" + escapeUri(value)]); }, [])
                .join("&");
        }
    };
    try {
        for (var _d = __values(Object.keys(query).sort()), _e = _d.next(); !_e.done; _e = _d.next()) {
            var key = _e.value;
            _loop_1(key);
        }
    }
    catch (e_1_1) { e_1 = { error: e_1_1 }; }
    finally {
        try {
            if (_e && !_e.done && (_b = _d.return)) _b.call(_d);
        }
        finally { if (e_1) throw e_1.error; }
    }
    return keys
        .map(function (key) { return serialized[key]; })
        .filter(function (serialized) { return serialized; }) // omit any falsy values
        .join("&");
}

var isArrayBuffer = function (arg) {
    return (typeof ArrayBuffer === "function" && arg instanceof ArrayBuffer) ||
        Object.prototype.toString.call(arg) === "[object ArrayBuffer]";
};

/**
 * @internal
 */
function getPayloadHash(_a, hashConstructor) {
    var headers = _a.headers, body = _a.body;
    return __awaiter$7(this, void 0, void 0, function () {
        var _b, _c, headerName, hashCtor, _d;
        var e_1, _e;
        return __generator$7(this, function (_f) {
            switch (_f.label) {
                case 0:
                    try {
                        for (_b = __values(Object.keys(headers)), _c = _b.next(); !_c.done; _c = _b.next()) {
                            headerName = _c.value;
                            if (headerName.toLowerCase() === SHA256_HEADER) {
                                return [2 /*return*/, headers[headerName]];
                            }
                        }
                    }
                    catch (e_1_1) { e_1 = { error: e_1_1 }; }
                    finally {
                        try {
                            if (_c && !_c.done && (_e = _b.return)) _e.call(_b);
                        }
                        finally { if (e_1) throw e_1.error; }
                    }
                    if (!(body == undefined)) return [3 /*break*/, 1];
                    return [2 /*return*/, "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"];
                case 1:
                    if (!(typeof body === "string" || ArrayBuffer.isView(body) || isArrayBuffer(body))) return [3 /*break*/, 3];
                    hashCtor = new hashConstructor();
                    hashCtor.update(body);
                    _d = toHex$1;
                    return [4 /*yield*/, hashCtor.digest()];
                case 2: return [2 /*return*/, _d.apply(void 0, [_f.sent()])];
                case 3: 
                // As any defined body that is not a string or binary data is a stream, this
                // body is unsignable. Attempt to send the request with an unsigned payload,
                // which may or may not be accepted by the service.
                return [2 /*return*/, UNSIGNED_PAYLOAD];
            }
        });
    });
}

function hasHeader(soughtHeader, headers) {
    var e_1, _a;
    soughtHeader = soughtHeader.toLowerCase();
    try {
        for (var _b = __values(Object.keys(headers)), _c = _b.next(); !_c.done; _c = _b.next()) {
            var headerName = _c.value;
            if (soughtHeader === headerName.toLowerCase()) {
                return true;
            }
        }
    }
    catch (e_1_1) { e_1 = { error: e_1_1 }; }
    finally {
        try {
            if (_c && !_c.done && (_a = _b.return)) _a.call(_b);
        }
        finally { if (e_1) throw e_1.error; }
    }
    return false;
}

/**
 * @internal
 */
function cloneRequest(_a) {
    var headers = _a.headers, query = _a.query, rest = __rest(_a, ["headers", "query"]);
    return __assign$6(__assign$6({}, rest), { headers: __assign$6({}, headers), query: query ? cloneQuery(query) : undefined });
}
function cloneQuery(query) {
    return Object.keys(query).reduce(function (carry, paramName) {
        var _a;
        var param = query[paramName];
        return __assign$6(__assign$6({}, carry), (_a = {}, _a[paramName] = Array.isArray(param) ? __spread$1(param) : param, _a));
    }, {});
}

/**
 * @internal
 */
function moveHeadersToQuery(request, options) {
    var e_1, _a;
    var _b;
    if (options === void 0) { options = {}; }
    var _c = typeof request.clone === "function" ? request.clone() : cloneRequest(request), headers = _c.headers, _d = _c.query, query = _d === void 0 ? {} : _d;
    try {
        for (var _e = __values(Object.keys(headers)), _f = _e.next(); !_f.done; _f = _e.next()) {
            var name = _f.value;
            var lname = name.toLowerCase();
            if (lname.substr(0, 6) === "x-amz-" && !((_b = options.unhoistableHeaders) === null || _b === void 0 ? void 0 : _b.has(lname))) {
                query[name] = headers[name];
                delete headers[name];
            }
        }
    }
    catch (e_1_1) { e_1 = { error: e_1_1 }; }
    finally {
        try {
            if (_f && !_f.done && (_a = _e.return)) _a.call(_e);
        }
        finally { if (e_1) throw e_1.error; }
    }
    return __assign$6(__assign$6({}, request), { headers: headers,
        query: query });
}

/**
 * @internal
 */
function prepareRequest(request) {
    var e_1, _a;
    // Create a clone of the request object that does not clone the body
    request = typeof request.clone === "function" ? request.clone() : cloneRequest(request);
    try {
        for (var _b = __values(Object.keys(request.headers)), _c = _b.next(); !_c.done; _c = _b.next()) {
            var headerName = _c.value;
            if (GENERATED_HEADERS.indexOf(headerName.toLowerCase()) > -1) {
                delete request.headers[headerName];
            }
        }
    }
    catch (e_1_1) { e_1 = { error: e_1_1 }; }
    finally {
        try {
            if (_c && !_c.done && (_a = _b.return)) _a.call(_b);
        }
        finally { if (e_1) throw e_1.error; }
    }
    return request;
}

function iso8601(time) {
    return toDate(time)
        .toISOString()
        .replace(/\.\d{3}Z$/, "Z");
}
function toDate(time) {
    if (typeof time === "number") {
        return new Date(time * 1000);
    }
    if (typeof time === "string") {
        if (Number(time)) {
            return new Date(Number(time) * 1000);
        }
        return new Date(time);
    }
    return time;
}

var SignatureV4 = /** @class */ (function () {
    function SignatureV4(_a) {
        var applyChecksum = _a.applyChecksum, credentials = _a.credentials, region = _a.region, service = _a.service, sha256 = _a.sha256, _b = _a.uriEscapePath, uriEscapePath = _b === void 0 ? true : _b;
        this.service = service;
        this.sha256 = sha256;
        this.uriEscapePath = uriEscapePath;
        // default to true if applyChecksum isn't set
        this.applyChecksum = typeof applyChecksum === "boolean" ? applyChecksum : true;
        this.regionProvider = normalizeRegionProvider(region);
        this.credentialProvider = normalizeCredentialsProvider(credentials);
    }
    SignatureV4.prototype.presign = function (originalRequest, options) {
        if (options === void 0) { options = {}; }
        return __awaiter$7(this, void 0, void 0, function () {
            var _a, signingDate, _b, expiresIn, unsignableHeaders, unhoistableHeaders, signableHeaders, signingRegion, signingService, credentials, region, _c, _d, longDate, shortDate, scope, request, canonicalHeaders, _e, _f, _g, _h, _j, _k;
            return __generator$7(this, function (_l) {
                switch (_l.label) {
                    case 0:
                        _a = options.signingDate, signingDate = _a === void 0 ? new Date() : _a, _b = options.expiresIn, expiresIn = _b === void 0 ? 3600 : _b, unsignableHeaders = options.unsignableHeaders, unhoistableHeaders = options.unhoistableHeaders, signableHeaders = options.signableHeaders, signingRegion = options.signingRegion, signingService = options.signingService;
                        return [4 /*yield*/, this.credentialProvider()];
                    case 1:
                        credentials = _l.sent();
                        if (!(signingRegion !== null && signingRegion !== void 0)) return [3 /*break*/, 2];
                        _c = signingRegion;
                        return [3 /*break*/, 4];
                    case 2: return [4 /*yield*/, this.regionProvider()];
                    case 3:
                        _c = (_l.sent());
                        _l.label = 4;
                    case 4:
                        region = _c;
                        _d = formatDate(signingDate), longDate = _d.longDate, shortDate = _d.shortDate;
                        if (expiresIn > MAX_PRESIGNED_TTL) {
                            return [2 /*return*/, Promise.reject("Signature version 4 presigned URLs" + " must have an expiration date less than one week in" + " the future")];
                        }
                        scope = createScope(shortDate, region, signingService !== null && signingService !== void 0 ? signingService : this.service);
                        request = moveHeadersToQuery(prepareRequest(originalRequest), { unhoistableHeaders: unhoistableHeaders });
                        if (credentials.sessionToken) {
                            request.query[TOKEN_QUERY_PARAM] = credentials.sessionToken;
                        }
                        request.query[ALGORITHM_QUERY_PARAM] = ALGORITHM_IDENTIFIER;
                        request.query[CREDENTIAL_QUERY_PARAM] = credentials.accessKeyId + "/" + scope;
                        request.query[AMZ_DATE_QUERY_PARAM] = longDate;
                        request.query[EXPIRES_QUERY_PARAM] = expiresIn.toString(10);
                        canonicalHeaders = getCanonicalHeaders(request, unsignableHeaders, signableHeaders);
                        request.query[SIGNED_HEADERS_QUERY_PARAM] = getCanonicalHeaderList(canonicalHeaders);
                        _e = request.query;
                        _f = SIGNATURE_QUERY_PARAM;
                        _g = this.getSignature;
                        _h = [longDate,
                            scope,
                            this.getSigningKey(credentials, region, shortDate, signingService)];
                        _j = this.createCanonicalRequest;
                        _k = [request, canonicalHeaders];
                        return [4 /*yield*/, getPayloadHash(originalRequest, this.sha256)];
                    case 5: return [4 /*yield*/, _g.apply(this, _h.concat([_j.apply(this, _k.concat([_l.sent()]))]))];
                    case 6:
                        _e[_f] = _l.sent();
                        return [2 /*return*/, request];
                }
            });
        });
    };
    SignatureV4.prototype.sign = function (toSign, options) {
        return __awaiter$7(this, void 0, void 0, function () {
            return __generator$7(this, function (_a) {
                if (typeof toSign === "string") {
                    return [2 /*return*/, this.signString(toSign, options)];
                }
                else if (toSign.headers && toSign.payload) {
                    return [2 /*return*/, this.signEvent(toSign, options)];
                }
                else {
                    return [2 /*return*/, this.signRequest(toSign, options)];
                }
            });
        });
    };
    SignatureV4.prototype.signEvent = function (_a, _b) {
        var headers = _a.headers, payload = _a.payload;
        var _c = _b.signingDate, signingDate = _c === void 0 ? new Date() : _c, priorSignature = _b.priorSignature, signingRegion = _b.signingRegion, signingService = _b.signingService;
        return __awaiter$7(this, void 0, void 0, function () {
            var region, _d, _e, shortDate, longDate, scope, hashedPayload, hash, hashedHeaders, _f, stringToSign;
            return __generator$7(this, function (_g) {
                switch (_g.label) {
                    case 0:
                        if (!(signingRegion !== null && signingRegion !== void 0)) return [3 /*break*/, 1];
                        _d = signingRegion;
                        return [3 /*break*/, 3];
                    case 1: return [4 /*yield*/, this.regionProvider()];
                    case 2:
                        _d = (_g.sent());
                        _g.label = 3;
                    case 3:
                        region = _d;
                        _e = formatDate(signingDate), shortDate = _e.shortDate, longDate = _e.longDate;
                        scope = createScope(shortDate, region, signingService !== null && signingService !== void 0 ? signingService : this.service);
                        return [4 /*yield*/, getPayloadHash({ headers: {}, body: payload }, this.sha256)];
                    case 4:
                        hashedPayload = _g.sent();
                        hash = new this.sha256();
                        hash.update(headers);
                        _f = toHex$1;
                        return [4 /*yield*/, hash.digest()];
                    case 5:
                        hashedHeaders = _f.apply(void 0, [_g.sent()]);
                        stringToSign = [
                            EVENT_ALGORITHM_IDENTIFIER,
                            longDate,
                            scope,
                            priorSignature,
                            hashedHeaders,
                            hashedPayload,
                        ].join("\n");
                        return [2 /*return*/, this.signString(stringToSign, { signingDate: signingDate, signingRegion: region, signingService: signingService })];
                }
            });
        });
    };
    SignatureV4.prototype.signString = function (stringToSign, _a) {
        var _b = _a === void 0 ? {} : _a, _c = _b.signingDate, signingDate = _c === void 0 ? new Date() : _c, signingRegion = _b.signingRegion, signingService = _b.signingService;
        return __awaiter$7(this, void 0, void 0, function () {
            var credentials, region, _d, shortDate, hash, _e, _f, _g;
            return __generator$7(this, function (_h) {
                switch (_h.label) {
                    case 0: return [4 /*yield*/, this.credentialProvider()];
                    case 1:
                        credentials = _h.sent();
                        if (!(signingRegion !== null && signingRegion !== void 0)) return [3 /*break*/, 2];
                        _d = signingRegion;
                        return [3 /*break*/, 4];
                    case 2: return [4 /*yield*/, this.regionProvider()];
                    case 3:
                        _d = (_h.sent());
                        _h.label = 4;
                    case 4:
                        region = _d;
                        shortDate = formatDate(signingDate).shortDate;
                        _f = (_e = this.sha256).bind;
                        return [4 /*yield*/, this.getSigningKey(credentials, region, shortDate, signingService)];
                    case 5:
                        hash = new (_f.apply(_e, [void 0, _h.sent()]))();
                        hash.update(stringToSign);
                        _g = toHex$1;
                        return [4 /*yield*/, hash.digest()];
                    case 6: return [2 /*return*/, _g.apply(void 0, [_h.sent()])];
                }
            });
        });
    };
    SignatureV4.prototype.signRequest = function (requestToSign, _a) {
        var _b = _a === void 0 ? {} : _a, _c = _b.signingDate, signingDate = _c === void 0 ? new Date() : _c, signableHeaders = _b.signableHeaders, unsignableHeaders = _b.unsignableHeaders, signingRegion = _b.signingRegion, signingService = _b.signingService;
        return __awaiter$7(this, void 0, void 0, function () {
            var credentials, region, _d, request, _e, longDate, shortDate, scope, payloadHash, canonicalHeaders, signature;
            return __generator$7(this, function (_f) {
                switch (_f.label) {
                    case 0: return [4 /*yield*/, this.credentialProvider()];
                    case 1:
                        credentials = _f.sent();
                        if (!(signingRegion !== null && signingRegion !== void 0)) return [3 /*break*/, 2];
                        _d = signingRegion;
                        return [3 /*break*/, 4];
                    case 2: return [4 /*yield*/, this.regionProvider()];
                    case 3:
                        _d = (_f.sent());
                        _f.label = 4;
                    case 4:
                        region = _d;
                        request = prepareRequest(requestToSign);
                        _e = formatDate(signingDate), longDate = _e.longDate, shortDate = _e.shortDate;
                        scope = createScope(shortDate, region, signingService !== null && signingService !== void 0 ? signingService : this.service);
                        request.headers[AMZ_DATE_HEADER] = longDate;
                        if (credentials.sessionToken) {
                            request.headers[TOKEN_HEADER] = credentials.sessionToken;
                        }
                        return [4 /*yield*/, getPayloadHash(request, this.sha256)];
                    case 5:
                        payloadHash = _f.sent();
                        if (!hasHeader(SHA256_HEADER, request.headers) && this.applyChecksum) {
                            request.headers[SHA256_HEADER] = payloadHash;
                        }
                        canonicalHeaders = getCanonicalHeaders(request, unsignableHeaders, signableHeaders);
                        return [4 /*yield*/, this.getSignature(longDate, scope, this.getSigningKey(credentials, region, shortDate, signingService), this.createCanonicalRequest(request, canonicalHeaders, payloadHash))];
                    case 6:
                        signature = _f.sent();
                        request.headers[AUTH_HEADER] =
                            ALGORITHM_IDENTIFIER + " " +
                                ("Credential=" + credentials.accessKeyId + "/" + scope + ", ") +
                                ("SignedHeaders=" + getCanonicalHeaderList(canonicalHeaders) + ", ") +
                                ("Signature=" + signature);
                        return [2 /*return*/, request];
                }
            });
        });
    };
    SignatureV4.prototype.createCanonicalRequest = function (request, canonicalHeaders, payloadHash) {
        var sortedHeaders = Object.keys(canonicalHeaders).sort();
        return request.method + "\n" + this.getCanonicalPath(request) + "\n" + getCanonicalQuery(request) + "\n" + sortedHeaders.map(function (name) { return name + ":" + canonicalHeaders[name]; }).join("\n") + "\n\n" + sortedHeaders.join(";") + "\n" + payloadHash;
    };
    SignatureV4.prototype.createStringToSign = function (longDate, credentialScope, canonicalRequest) {
        return __awaiter$7(this, void 0, void 0, function () {
            var hash, hashedRequest;
            return __generator$7(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        hash = new this.sha256();
                        hash.update(canonicalRequest);
                        return [4 /*yield*/, hash.digest()];
                    case 1:
                        hashedRequest = _a.sent();
                        return [2 /*return*/, ALGORITHM_IDENTIFIER + "\n" + longDate + "\n" + credentialScope + "\n" + toHex$1(hashedRequest)];
                }
            });
        });
    };
    SignatureV4.prototype.getCanonicalPath = function (_a) {
        var path = _a.path;
        if (this.uriEscapePath) {
            var doubleEncoded = encodeURIComponent(path.replace(/^\//, ""));
            return "/" + doubleEncoded.replace(/%2F/g, "/");
        }
        return path;
    };
    SignatureV4.prototype.getSignature = function (longDate, credentialScope, keyPromise, canonicalRequest) {
        return __awaiter$7(this, void 0, void 0, function () {
            var stringToSign, hash, _a, _b, _c;
            return __generator$7(this, function (_d) {
                switch (_d.label) {
                    case 0: return [4 /*yield*/, this.createStringToSign(longDate, credentialScope, canonicalRequest)];
                    case 1:
                        stringToSign = _d.sent();
                        _b = (_a = this.sha256).bind;
                        return [4 /*yield*/, keyPromise];
                    case 2:
                        hash = new (_b.apply(_a, [void 0, _d.sent()]))();
                        hash.update(stringToSign);
                        _c = toHex$1;
                        return [4 /*yield*/, hash.digest()];
                    case 3: return [2 /*return*/, _c.apply(void 0, [_d.sent()])];
                }
            });
        });
    };
    SignatureV4.prototype.getSigningKey = function (credentials, region, shortDate, service) {
        return getSigningKey(this.sha256, credentials, shortDate, region, service || this.service);
    };
    return SignatureV4;
}());
var formatDate = function (now) {
    var longDate = iso8601(now).replace(/[\-:]/g, "");
    return {
        longDate: longDate,
        shortDate: longDate.substr(0, 8),
    };
};
var getCanonicalHeaderList = function (headers) { return Object.keys(headers).sort().join(";"); };
var normalizeRegionProvider = function (region) {
    if (typeof region === "string") {
        var promisified_1 = Promise.resolve(region);
        return function () { return promisified_1; };
    }
    else {
        return region;
    }
};
var normalizeCredentialsProvider = function (credentials) {
    if (typeof credentials === "object") {
        var promisified_2 = Promise.resolve(credentials);
        return function () { return promisified_2; };
    }
    else {
        return credentials;
    }
};

function resolveAwsAuthConfig(input) {
    var _this = this;
    var credentials = input.credentials || input.credentialDefaultProvider(input);
    var normalizedCreds = normalizeProvider(credentials);
    var _a = input.signingEscapePath, signingEscapePath = _a === void 0 ? true : _a, _b = input.systemClockOffset, systemClockOffset = _b === void 0 ? input.systemClockOffset || 0 : _b, sha256 = input.sha256;
    var signer;
    if (input.signer) {
        //if signer is supplied by user, normalize it to a function returning a promise for signer.
        signer = normalizeProvider(input.signer);
    }
    else {
        //construct a provider inferring signing from region.
        signer = function () {
            return normalizeProvider(input.region)()
                .then(function (region) { return __awaiter$7(_this, void 0, void 0, function () { return __generator$7(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, input.regionInfoProvider(region)];
                    case 1: return [2 /*return*/, [(_a.sent()) || {}, region]];
                }
            }); }); })
                .then(function (_a) {
                var _b = __read$4(_a, 2), regionInfo = _b[0], region = _b[1];
                var signingRegion = regionInfo.signingRegion, signingService = regionInfo.signingService;
                //update client's singing region and signing service config if they are resolved.
                //signing region resolving order: user supplied signingRegion -> endpoints.json inferred region -> client region
                input.signingRegion = input.signingRegion || signingRegion || region;
                //signing name resolving order:
                //user supplied signingName -> endpoints.json inferred (credential scope -> model arnNamespace) -> model service id
                input.signingName = input.signingName || signingService || input.serviceId;
                return new SignatureV4({
                    credentials: normalizedCreds,
                    region: input.signingRegion,
                    service: input.signingName,
                    sha256: sha256,
                    uriEscapePath: signingEscapePath,
                });
            });
        };
    }
    return __assign$6(__assign$6({}, input), { systemClockOffset: systemClockOffset,
        signingEscapePath: signingEscapePath, credentials: normalizedCreds, signer: signer });
}
function normalizeProvider(input) {
    if (typeof input === "object") {
        var promisified_1 = Promise.resolve(input);
        return function () { return promisified_1; };
    }
    return input;
}

function resolveUserAgentConfig(input) {
    return __assign$6(__assign$6({}, input), { customUserAgent: typeof input.customUserAgent === "string" ? [[input.customUserAgent]] : input.customUserAgent });
}

var USER_AGENT = "user-agent";
var X_AMZ_USER_AGENT = "x-amz-user-agent";
var SPACE = " ";
var UA_ESCAPE_REGEX = /[^\!\#\$\%\&\'\*\+\-\.\^\_\`\|\~\d\w]/g;

/**
 * Build user agent header sections from:
 * 1. runtime-specific default user agent provider;
 * 2. custom user agent from `customUserAgent` client config;
 * 3. handler execution context set by internal SDK components;
 * The built user agent will be set to `x-amz-user-agent` header for ALL the
 * runtimes.
 * Please note that any override to the `user-agent` or `x-amz-user-agent` header
 * in the HTTP request is discouraged. Please use `customUserAgent` client
 * config or middleware setting the `userAgent` context to generate desired user
 * agent.
 */
var userAgentMiddleware = function (options) { return function (next, context) { return function (args) { return __awaiter$7(void 0, void 0, void 0, function () {
    var request, headers, userAgent, defaultUserAgent, customUserAgent, normalUAValue;
    var _a, _b;
    return __generator$7(this, function (_c) {
        switch (_c.label) {
            case 0:
                request = args.request;
                if (!HttpRequest.isInstance(request))
                    return [2 /*return*/, next(args)];
                headers = request.headers;
                userAgent = ((_a = context === null || context === void 0 ? void 0 : context.userAgent) === null || _a === void 0 ? void 0 : _a.map(escapeUserAgent)) || [];
                return [4 /*yield*/, options.defaultUserAgentProvider()];
            case 1:
                defaultUserAgent = (_c.sent()).map(escapeUserAgent);
                customUserAgent = ((_b = options === null || options === void 0 ? void 0 : options.customUserAgent) === null || _b === void 0 ? void 0 : _b.map(escapeUserAgent)) || [];
                // Set value to AWS-specific user agent header
                headers[X_AMZ_USER_AGENT] = __spread$1(defaultUserAgent, userAgent, customUserAgent).join(SPACE);
                normalUAValue = __spread$1(defaultUserAgent.filter(function (section) { return section.startsWith("aws-sdk-"); }), customUserAgent).join(SPACE);
                if (options.runtime !== "browser" && normalUAValue) {
                    headers[USER_AGENT] = headers[USER_AGENT] ? headers[USER_AGENT] + " " + normalUAValue : normalUAValue;
                }
                return [2 /*return*/, next(__assign$6(__assign$6({}, args), { request: request }))];
        }
    });
}); }; }; };
/**
 * Escape the each pair according to https://tools.ietf.org/html/rfc5234 and join the pair with pattern `name/version`.
 * User agent name may include prefix like `md/`, `api/`, `os/` etc., we should not escape the `/` after the prefix.
 * @private
 */
var escapeUserAgent = function (_a) {
    var _b = __read$4(_a, 2), name = _b[0], version = _b[1];
    var prefixSeparatorIndex = name.indexOf("/");
    var prefix = name.substring(0, prefixSeparatorIndex); // If no prefix, prefix is just ""
    var uaName = name.substring(prefixSeparatorIndex + 1);
    if (prefix === "api") {
        uaName = uaName.toLowerCase();
    }
    return [prefix, uaName, version]
        .filter(function (item) { return item && item.length > 0; })
        .map(function (item) { return item === null || item === void 0 ? void 0 : item.replace(UA_ESCAPE_REGEX, "_"); })
        .join("/");
};
var getUserAgentMiddlewareOptions = {
    name: "getUserAgentMiddleware",
    step: "build",
    priority: "low",
    tags: ["SET_USER_AGENT", "USER_AGENT"],
    override: true,
};
var getUserAgentPlugin = function (config) { return ({
    applyToStack: function (clientStack) {
        clientStack.add(userAgentMiddleware(config), getUserAgentMiddlewareOptions);
    },
}); };

var constructStack = function () {
    var absoluteEntries = [];
    var relativeEntries = [];
    var entriesNameSet = new Set();
    var sort = function (entries) {
        return entries.sort(function (a, b) {
            return stepWeights[b.step] - stepWeights[a.step] ||
                priorityWeights[b.priority || "normal"] - priorityWeights[a.priority || "normal"];
        });
    };
    var removeByName = function (toRemove) {
        var isRemoved = false;
        var filterCb = function (entry) {
            if (entry.name && entry.name === toRemove) {
                isRemoved = true;
                entriesNameSet.delete(toRemove);
                return false;
            }
            return true;
        };
        absoluteEntries = absoluteEntries.filter(filterCb);
        relativeEntries = relativeEntries.filter(filterCb);
        return isRemoved;
    };
    var removeByReference = function (toRemove) {
        var isRemoved = false;
        var filterCb = function (entry) {
            if (entry.middleware === toRemove) {
                isRemoved = true;
                if (entry.name)
                    entriesNameSet.delete(entry.name);
                return false;
            }
            return true;
        };
        absoluteEntries = absoluteEntries.filter(filterCb);
        relativeEntries = relativeEntries.filter(filterCb);
        return isRemoved;
    };
    var cloneTo = function (toStack) {
        absoluteEntries.forEach(function (entry) {
            //@ts-ignore
            toStack.add(entry.middleware, __assign$6({}, entry));
        });
        relativeEntries.forEach(function (entry) {
            //@ts-ignore
            toStack.addRelativeTo(entry.middleware, __assign$6({}, entry));
        });
        return toStack;
    };
    var expandRelativeMiddlewareList = function (from) {
        var expandedMiddlewareList = [];
        from.before.forEach(function (entry) {
            if (entry.before.length === 0 && entry.after.length === 0) {
                expandedMiddlewareList.push(entry);
            }
            else {
                expandedMiddlewareList.push.apply(expandedMiddlewareList, __spread$1(expandRelativeMiddlewareList(entry)));
            }
        });
        expandedMiddlewareList.push(from);
        from.after.reverse().forEach(function (entry) {
            if (entry.before.length === 0 && entry.after.length === 0) {
                expandedMiddlewareList.push(entry);
            }
            else {
                expandedMiddlewareList.push.apply(expandedMiddlewareList, __spread$1(expandRelativeMiddlewareList(entry)));
            }
        });
        return expandedMiddlewareList;
    };
    /**
     * Get a final list of middleware in the order of being executed in the resolved handler.
     */
    var getMiddlewareList = function () {
        var normalizedAbsoluteEntries = [];
        var normalizedRelativeEntries = [];
        var normalizedEntriesNameMap = {};
        absoluteEntries.forEach(function (entry) {
            var normalizedEntry = __assign$6(__assign$6({}, entry), { before: [], after: [] });
            if (normalizedEntry.name)
                normalizedEntriesNameMap[normalizedEntry.name] = normalizedEntry;
            normalizedAbsoluteEntries.push(normalizedEntry);
        });
        relativeEntries.forEach(function (entry) {
            var normalizedEntry = __assign$6(__assign$6({}, entry), { before: [], after: [] });
            if (normalizedEntry.name)
                normalizedEntriesNameMap[normalizedEntry.name] = normalizedEntry;
            normalizedRelativeEntries.push(normalizedEntry);
        });
        normalizedRelativeEntries.forEach(function (entry) {
            if (entry.toMiddleware) {
                var toMiddleware = normalizedEntriesNameMap[entry.toMiddleware];
                if (toMiddleware === undefined) {
                    throw new Error(entry.toMiddleware + " is not found when adding " + (entry.name || "anonymous") + " middleware " + entry.relation + " " + entry.toMiddleware);
                }
                if (entry.relation === "after") {
                    toMiddleware.after.push(entry);
                }
                if (entry.relation === "before") {
                    toMiddleware.before.push(entry);
                }
            }
        });
        var mainChain = sort(normalizedAbsoluteEntries)
            .map(expandRelativeMiddlewareList)
            .reduce(function (wholeList, expendedMiddlewareList) {
            // TODO: Replace it with Array.flat();
            wholeList.push.apply(wholeList, __spread$1(expendedMiddlewareList));
            return wholeList;
        }, []);
        return mainChain.map(function (entry) { return entry.middleware; });
    };
    var stack = {
        add: function (middleware, options) {
            if (options === void 0) { options = {}; }
            var name = options.name, override = options.override;
            var entry = __assign$6({ step: "initialize", priority: "normal", middleware: middleware }, options);
            if (name) {
                if (entriesNameSet.has(name)) {
                    if (!override)
                        throw new Error("Duplicate middleware name '" + name + "'");
                    var toOverrideIndex = absoluteEntries.findIndex(function (entry) { return entry.name === name; });
                    var toOverride = absoluteEntries[toOverrideIndex];
                    if (toOverride.step !== entry.step || toOverride.priority !== entry.priority) {
                        throw new Error("\"" + name + "\" middleware with " + toOverride.priority + " priority in " + toOverride.step + " step cannot be " +
                            ("overridden by same-name middleware with " + entry.priority + " priority in " + entry.step + " step."));
                    }
                    absoluteEntries.splice(toOverrideIndex, 1);
                }
                entriesNameSet.add(name);
            }
            absoluteEntries.push(entry);
        },
        addRelativeTo: function (middleware, options) {
            var name = options.name, override = options.override;
            var entry = __assign$6({ middleware: middleware }, options);
            if (name) {
                if (entriesNameSet.has(name)) {
                    if (!override)
                        throw new Error("Duplicate middleware name '" + name + "'");
                    var toOverrideIndex = relativeEntries.findIndex(function (entry) { return entry.name === name; });
                    var toOverride = relativeEntries[toOverrideIndex];
                    if (toOverride.toMiddleware !== entry.toMiddleware || toOverride.relation !== entry.relation) {
                        throw new Error("\"" + name + "\" middleware " + toOverride.relation + " \"" + toOverride.toMiddleware + "\" middleware cannot be overridden " +
                            ("by same-name middleware " + entry.relation + " \"" + entry.toMiddleware + "\" middleware."));
                    }
                    relativeEntries.splice(toOverrideIndex, 1);
                }
                entriesNameSet.add(name);
            }
            relativeEntries.push(entry);
        },
        clone: function () { return cloneTo(constructStack()); },
        use: function (plugin) {
            plugin.applyToStack(stack);
        },
        remove: function (toRemove) {
            if (typeof toRemove === "string")
                return removeByName(toRemove);
            else
                return removeByReference(toRemove);
        },
        removeByTag: function (toRemove) {
            var isRemoved = false;
            var filterCb = function (entry) {
                var tags = entry.tags, name = entry.name;
                if (tags && tags.includes(toRemove)) {
                    if (name)
                        entriesNameSet.delete(name);
                    isRemoved = true;
                    return false;
                }
                return true;
            };
            absoluteEntries = absoluteEntries.filter(filterCb);
            relativeEntries = relativeEntries.filter(filterCb);
            return isRemoved;
        },
        concat: function (from) {
            var cloned = cloneTo(constructStack());
            cloned.use(from);
            return cloned;
        },
        applyToStack: cloneTo,
        resolve: function (handler, context) {
            var e_1, _a;
            try {
                for (var _b = __values(getMiddlewareList().reverse()), _c = _b.next(); !_c.done; _c = _b.next()) {
                    var middleware = _c.value;
                    handler = middleware(handler, context);
                }
            }
            catch (e_1_1) { e_1 = { error: e_1_1 }; }
            finally {
                try {
                    if (_c && !_c.done && (_a = _b.return)) _a.call(_b);
                }
                finally { if (e_1) throw e_1.error; }
            }
            return handler;
        },
    };
    return stack;
};
var stepWeights = {
    initialize: 5,
    serialize: 4,
    build: 3,
    finalizeRequest: 2,
    deserialize: 1,
};
var priorityWeights = {
    high: 3,
    normal: 2,
    low: 1,
};

var Client$1 = /** @class */ (function () {
    function Client(config) {
        this.middlewareStack = constructStack();
        this.config = config;
    }
    Client.prototype.send = function (command, optionsOrCb, cb) {
        var options = typeof optionsOrCb !== "function" ? optionsOrCb : undefined;
        var callback = typeof optionsOrCb === "function" ? optionsOrCb : cb;
        var handler = command.resolveMiddleware(this.middlewareStack, this.config, options);
        if (callback) {
            handler(command)
                .then(function (result) { return callback(null, result.output); }, function (err) { return callback(err); })
                .catch(
            // prevent any errors thrown in the callback from triggering an
            // unhandled promise rejection
            function () { });
        }
        else {
            return handler(command).then(function (result) { return result.output; });
        }
    };
    Client.prototype.destroy = function () {
        if (this.config.requestHandler.destroy)
            this.config.requestHandler.destroy();
    };
    return Client;
}());

var Command = /** @class */ (function () {
    function Command() {
        this.middlewareStack = constructStack();
    }
    return Command;
}());

/**
 * Lazy String holder for JSON typed contents.
 */
/**
 * Because of https://github.com/microsoft/tslib/issues/95,
 * TS 'extends' shim doesn't support extending native types like String.
 * So here we create StringWrapper that duplicate everything from String
 * class including its prototype chain. So we can extend from here.
 */
// @ts-ignore StringWrapper implementation is not a simple constructor
var StringWrapper = function () {
    //@ts-ignore 'this' cannot be assigned to any, but Object.getPrototypeOf accepts any
    var Class = Object.getPrototypeOf(this).constructor;
    var Constructor = Function.bind.apply(String, __spread$1([null], arguments));
    //@ts-ignore Call wrapped String constructor directly, don't bother typing it.
    var instance = new Constructor();
    Object.setPrototypeOf(instance, Class.prototype);
    return instance;
};
StringWrapper.prototype = Object.create(String.prototype, {
    constructor: {
        value: StringWrapper,
        enumerable: false,
        writable: true,
        configurable: true,
    },
});
Object.setPrototypeOf(StringWrapper, String);
/** @class */ ((function (_super) {
    __extends$3(LazyJsonString, _super);
    function LazyJsonString() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    LazyJsonString.prototype.deserializeJSON = function () {
        return JSON.parse(_super.prototype.toString.call(this));
    };
    LazyJsonString.prototype.toJSON = function () {
        return _super.prototype.toString.call(this);
    };
    LazyJsonString.fromObject = function (object) {
        if (object instanceof LazyJsonString) {
            return object;
        }
        else if (object instanceof String || typeof object === "string") {
            return new LazyJsonString(object);
        }
        return new LazyJsonString(JSON.stringify(object));
    };
    return LazyJsonString;
})(StringWrapper));

var deserializerMiddleware = function (options, deserializer) { return function (next, context) { return function (args) { return __awaiter$7(void 0, void 0, void 0, function () {
    var response, parsed;
    return __generator$7(this, function (_a) {
        switch (_a.label) {
            case 0: return [4 /*yield*/, next(args)];
            case 1:
                response = (_a.sent()).response;
                return [4 /*yield*/, deserializer(response, options)];
            case 2:
                parsed = _a.sent();
                return [2 /*return*/, {
                        response: response,
                        output: parsed,
                    }];
        }
    });
}); }; }; };

var serializerMiddleware = function (options, serializer) { return function (next, context) { return function (args) { return __awaiter$7(void 0, void 0, void 0, function () {
    var request;
    return __generator$7(this, function (_a) {
        switch (_a.label) {
            case 0: return [4 /*yield*/, serializer(args.input, options)];
            case 1:
                request = _a.sent();
                return [2 /*return*/, next(__assign$6(__assign$6({}, args), { request: request }))];
        }
    });
}); }; }; };

var deserializerMiddlewareOption = {
    name: "deserializerMiddleware",
    step: "deserialize",
    tags: ["DESERIALIZER"],
    override: true,
};
var serializerMiddlewareOption = {
    name: "serializerMiddleware",
    step: "serialize",
    tags: ["SERIALIZER"],
    override: true,
};
function getSerdePlugin(config, serializer, deserializer) {
    return {
        applyToStack: function (commandStack) {
            commandStack.add(deserializerMiddleware(config, deserializer), deserializerMiddlewareOption);
            commandStack.add(serializerMiddleware(config, serializer), serializerMiddlewareOption);
        },
    };
}

var __awaiter$5 = (undefined && undefined.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator$5 = (undefined && undefined.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
var logger$6 = new ConsoleLogger('CognitoCredentials');
var waitForInit$1 = new Promise(function (res, rej) {
    if (!browserOrNode().isBrowser) {
        logger$6.debug('not in the browser, directly resolved');
        return res();
    }
    var ga = window['gapi'] && window['gapi'].auth2 ? window['gapi'].auth2 : null;
    if (ga) {
        logger$6.debug('google api already loaded');
        return res();
    }
    else {
        setTimeout(function () {
            return res();
        }, 2000);
    }
});
var GoogleOAuth$1 = /** @class */ (function () {
    function GoogleOAuth() {
        this.initialized = false;
        this.refreshGoogleToken = this.refreshGoogleToken.bind(this);
        this._refreshGoogleTokenImpl = this._refreshGoogleTokenImpl.bind(this);
    }
    GoogleOAuth.prototype.refreshGoogleToken = function () {
        return __awaiter$5(this, void 0, void 0, function () {
            return __generator$5(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        if (!!this.initialized) return [3 /*break*/, 2];
                        logger$6.debug('need to wait for the Google SDK loaded');
                        return [4 /*yield*/, waitForInit$1];
                    case 1:
                        _a.sent();
                        this.initialized = true;
                        logger$6.debug('finish waiting');
                        _a.label = 2;
                    case 2: return [2 /*return*/, this._refreshGoogleTokenImpl()];
                }
            });
        });
    };
    GoogleOAuth.prototype._refreshGoogleTokenImpl = function () {
        var ga = null;
        if (browserOrNode().isBrowser)
            ga = window['gapi'] && window['gapi'].auth2 ? window['gapi'].auth2 : null;
        if (!ga) {
            logger$6.debug('no gapi auth2 available');
            return Promise.reject('no gapi auth2 available');
        }
        return new Promise(function (res, rej) {
            ga.getAuthInstance()
                .then(function (googleAuth) {
                if (!googleAuth) {
                    logger$6.debug('google Auth undefined');
                    rej(new NonRetryableError('google Auth undefined'));
                }
                var googleUser = googleAuth.currentUser.get();
                // refresh the token
                if (googleUser.isSignedIn()) {
                    logger$6.debug('refreshing the google access token');
                    googleUser
                        .reloadAuthResponse()
                        .then(function (authResponse) {
                        var id_token = authResponse.id_token, expires_at = authResponse.expires_at;
                        res({ token: id_token, expires_at: expires_at });
                    })
                        .catch(function (err) {
                        if (err && err.error === 'network_error') {
                            // Not using NonRetryableError so handler will be retried
                            rej('Network error reloading google auth response');
                        }
                        else {
                            rej(new NonRetryableError('Failed to reload google auth response'));
                        }
                    });
                }
                else {
                    rej(new NonRetryableError('User is not signed in with Google'));
                }
            })
                .catch(function (err) {
                logger$6.debug('Failed to refresh google token', err);
                rej(new NonRetryableError('Failed to refresh google token'));
            });
        });
    };
    return GoogleOAuth;
}());

var __awaiter$4 = (undefined && undefined.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator$4 = (undefined && undefined.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
var logger$5 = new ConsoleLogger('CognitoCredentials');
var waitForInit = new Promise(function (res, rej) {
    if (!browserOrNode().isBrowser) {
        logger$5.debug('not in the browser, directly resolved');
        return res();
    }
    var fb = window['FB'];
    if (fb) {
        logger$5.debug('FB SDK already loaded');
        return res();
    }
    else {
        setTimeout(function () {
            return res();
        }, 2000);
    }
});
var FacebookOAuth$1 = /** @class */ (function () {
    function FacebookOAuth() {
        this.initialized = false;
        this.refreshFacebookToken = this.refreshFacebookToken.bind(this);
        this._refreshFacebookTokenImpl = this._refreshFacebookTokenImpl.bind(this);
    }
    FacebookOAuth.prototype.refreshFacebookToken = function () {
        return __awaiter$4(this, void 0, void 0, function () {
            return __generator$4(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        if (!!this.initialized) return [3 /*break*/, 2];
                        logger$5.debug('need to wait for the Facebook SDK loaded');
                        return [4 /*yield*/, waitForInit];
                    case 1:
                        _a.sent();
                        this.initialized = true;
                        logger$5.debug('finish waiting');
                        _a.label = 2;
                    case 2: return [2 /*return*/, this._refreshFacebookTokenImpl()];
                }
            });
        });
    };
    FacebookOAuth.prototype._refreshFacebookTokenImpl = function () {
        var fb = null;
        if (browserOrNode().isBrowser)
            fb = window['FB'];
        if (!fb) {
            var errorMessage = 'no fb sdk available';
            logger$5.debug(errorMessage);
            return Promise.reject(new NonRetryableError(errorMessage));
        }
        return new Promise(function (res, rej) {
            fb.getLoginStatus(function (fbResponse) {
                if (!fbResponse || !fbResponse.authResponse) {
                    var errorMessage = 'no response from facebook when refreshing the jwt token';
                    logger$5.debug(errorMessage);
                    // There is no definitive indication for a network error in
                    // fbResponse, so we are treating it as an invalid token.
                    rej(new NonRetryableError(errorMessage));
                }
                else {
                    var response = fbResponse.authResponse;
                    var accessToken = response.accessToken, expiresIn = response.expiresIn;
                    var date = new Date();
                    var expires_at = expiresIn * 1000 + date.getTime();
                    if (!accessToken) {
                        var errorMessage = 'the jwtToken is undefined';
                        logger$5.debug(errorMessage);
                        rej(new NonRetryableError(errorMessage));
                    }
                    res({
                        token: accessToken,
                        expires_at: expires_at,
                    });
                }
            }, { scope: 'public_profile,email' });
        });
    };
    return FacebookOAuth;
}());

/*
 * Copyright 2017-2017 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"). You may not use this file except in compliance with
 * the License. A copy of the License is located at
 *
 *     http://aws.amazon.com/apache2.0/
 *
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
 * and limitations under the License.
 */
var GoogleOAuth = new GoogleOAuth$1();
var FacebookOAuth = new FacebookOAuth$1();

/*
 * Copyright 2017-2017 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"). You may not use this file except in compliance with
 * the License. A copy of the License is located at
 *
 *     http://aws.amazon.com/apache2.0/
 *
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
 * and limitations under the License.
 */
var dataMemory$1 = {};
/** @class */
var MemoryStorage$1 = /** @class */ (function () {
    function MemoryStorage() {
    }
    /**
     * This is used to set a specific item in storage
     * @param {string} key - the key for the item
     * @param {object} value - the value
     * @returns {string} value that was set
     */
    MemoryStorage.setItem = function (key, value) {
        dataMemory$1[key] = value;
        return dataMemory$1[key];
    };
    /**
     * This is used to get a specific key from storage
     * @param {string} key - the key for the item
     * This is used to clear the storage
     * @returns {string} the data item
     */
    MemoryStorage.getItem = function (key) {
        return Object.prototype.hasOwnProperty.call(dataMemory$1, key)
            ? dataMemory$1[key]
            : undefined;
    };
    /**
     * This is used to remove an item from storage
     * @param {string} key - the key being set
     * @returns {string} value - value that was deleted
     */
    MemoryStorage.removeItem = function (key) {
        return delete dataMemory$1[key];
    };
    /**
     * This is used to clear the storage
     * @returns {string} nothing
     */
    MemoryStorage.clear = function () {
        dataMemory$1 = {};
        return dataMemory$1;
    };
    return MemoryStorage;
}());
var StorageHelper$1 = /** @class */ (function () {
    /**
     * This is used to get a storage object
     * @returns {object} the storage
     */
    function StorageHelper() {
        try {
            this.storageWindow = window.localStorage;
            this.storageWindow.setItem('aws.amplify.test-ls', 1);
            this.storageWindow.removeItem('aws.amplify.test-ls');
        }
        catch (exception) {
            this.storageWindow = MemoryStorage$1;
        }
    }
    /**
     * This is used to return the storage
     * @returns {object} the storage
     */
    StorageHelper.prototype.getStorage = function () {
        return this.storageWindow;
    };
    return StorageHelper;
}());

/******************************************************************************
Copyright (c) Microsoft Corporation.

Permission to use, copy, modify, and/or distribute this software for any
purpose with or without fee is hereby granted.

THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH
REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY
AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM
LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR
OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR
PERFORMANCE OF THIS SOFTWARE.
***************************************************************************** */
/* global Reflect, Promise */

var extendStatics = function(d, b) {
    extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (Object.prototype.hasOwnProperty.call(b, p)) d[p] = b[p]; };
    return extendStatics(d, b);
};

function __extends$1(d, b) {
    if (typeof b !== "function" && b !== null)
        throw new TypeError("Class extends value " + String(b) + " is not a constructor or null");
    extendStatics(d, b);
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
}

var __assign$4 = function() {
    __assign$4 = Object.assign || function __assign(t) {
        for (var s, i = 1, n = arguments.length; i < n; i++) {
            s = arguments[i];
            for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p)) t[p] = s[p];
        }
        return t;
    };
    return __assign$4.apply(this, arguments);
};

function __awaiter$3(thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
}

function __generator$3(thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
}

function __read$2(o, n) {
    var m = typeof Symbol === "function" && o[Symbol.iterator];
    if (!m) return o;
    var i = m.call(o), r, ar = [], e;
    try {
        while ((n === void 0 || n-- > 0) && !(r = i.next()).done) ar.push(r.value);
    }
    catch (error) { e = { error: error }; }
    finally {
        try {
            if (r && !r.done && (m = i["return"])) m.call(i);
        }
        finally { if (e) throw e.error; }
    }
    return ar;
}

const name = "@aws-sdk/client-cognito-identity";
const description = "AWS SDK for JavaScript Cognito Identity Client for Node.js, Browser and React Native";
const version$1 = "3.6.1";
const scripts = {
  clean: "yarn remove-definitions && yarn remove-dist && yarn remove-documentation",
  "build-documentation": "yarn remove-documentation && typedoc ./",
  prepublishOnly: "yarn build",
  pretest: "yarn build:cjs",
  "remove-definitions": "rimraf ./types",
  "remove-dist": "rimraf ./dist",
  "remove-documentation": "rimraf ./docs",
  "test:unit": "mocha **/cjs/**/*.spec.js",
  "test:e2e": "mocha **/cjs/**/*.ispec.js && karma start karma.conf.js",
  test: "yarn test:unit",
  "build:cjs": "tsc -p tsconfig.json",
  "build:es": "tsc -p tsconfig.es.json",
  build: "yarn build:cjs && yarn build:es",
  postbuild: "downlevel-dts types types/ts3.4"
};
const main = "./dist/cjs/index.js";
const types = "./types/index.d.ts";
const module = "./dist/es/index.js";
const browser = {
  "./runtimeConfig": "./runtimeConfig.browser"
};
const sideEffects = false;
const dependencies = {
  "@aws-crypto/sha256-browser": "^1.0.0",
  "@aws-crypto/sha256-js": "^1.0.0",
  "@aws-sdk/config-resolver": "3.6.1",
  "@aws-sdk/credential-provider-node": "3.6.1",
  "@aws-sdk/fetch-http-handler": "3.6.1",
  "@aws-sdk/hash-node": "3.6.1",
  "@aws-sdk/invalid-dependency": "3.6.1",
  "@aws-sdk/middleware-content-length": "3.6.1",
  "@aws-sdk/middleware-host-header": "3.6.1",
  "@aws-sdk/middleware-logger": "3.6.1",
  "@aws-sdk/middleware-retry": "3.6.1",
  "@aws-sdk/middleware-serde": "3.6.1",
  "@aws-sdk/middleware-signing": "3.6.1",
  "@aws-sdk/middleware-stack": "3.6.1",
  "@aws-sdk/middleware-user-agent": "3.6.1",
  "@aws-sdk/node-config-provider": "3.6.1",
  "@aws-sdk/node-http-handler": "3.6.1",
  "@aws-sdk/protocol-http": "3.6.1",
  "@aws-sdk/smithy-client": "3.6.1",
  "@aws-sdk/types": "3.6.1",
  "@aws-sdk/url-parser": "3.6.1",
  "@aws-sdk/url-parser-native": "3.6.1",
  "@aws-sdk/util-base64-browser": "3.6.1",
  "@aws-sdk/util-base64-node": "3.6.1",
  "@aws-sdk/util-body-length-browser": "3.6.1",
  "@aws-sdk/util-body-length-node": "3.6.1",
  "@aws-sdk/util-user-agent-browser": "3.6.1",
  "@aws-sdk/util-user-agent-node": "3.6.1",
  "@aws-sdk/util-utf8-browser": "3.6.1",
  "@aws-sdk/util-utf8-node": "3.6.1",
  tslib: "^2.0.0"
};
const devDependencies = {
  "@aws-sdk/client-documentation-generator": "3.6.1",
  "@aws-sdk/client-iam": "3.6.1",
  "@types/chai": "^4.2.11",
  "@types/mocha": "^8.0.4",
  "@types/node": "^12.7.5",
  "downlevel-dts": "0.7.0",
  jest: "^26.1.0",
  rimraf: "^3.0.0",
  typedoc: "^0.19.2",
  typescript: "~4.1.2"
};
const engines = {
  node: ">=10.0.0"
};
const typesVersions = {
  "<4.0": {
    "types/*": [
      "types/ts3.4/*"
    ]
  }
};
const author = {
  name: "AWS SDK for JavaScript Team",
  url: "https://aws.amazon.com/javascript/"
};
const license = "Apache-2.0";
const homepage = "https://github.com/aws/aws-sdk-js-v3/tree/main/clients/client-cognito-identity";
const repository = {
  type: "git",
  url: "https://github.com/aws/aws-sdk-js-v3.git",
  directory: "clients/client-cognito-identity"
};
var packageInfo = {
  name: name,
  description: description,
  version: version$1,
  scripts: scripts,
  main: main,
  types: types,
  module: module,
  browser: browser,
  "react-native": {
  "./runtimeConfig": "./runtimeConfig.native"
},
  sideEffects: sideEffects,
  dependencies: dependencies,
  devDependencies: devDependencies,
  engines: engines,
  typesVersions: typesVersions,
  author: author,
  license: license,
  homepage: homepage,
  repository: repository
};

// Partition default templates
var AWS_TEMPLATE = "cognito-identity.{region}.amazonaws.com";
var AWS_CN_TEMPLATE = "cognito-identity.{region}.amazonaws.com.cn";
var AWS_ISO_TEMPLATE = "cognito-identity.{region}.c2s.ic.gov";
var AWS_ISO_B_TEMPLATE = "cognito-identity.{region}.sc2s.sgov.gov";
var AWS_US_GOV_TEMPLATE = "cognito-identity.{region}.amazonaws.com";
// Partition regions
var AWS_REGIONS = new Set([
    "af-south-1",
    "ap-east-1",
    "ap-northeast-1",
    "ap-northeast-2",
    "ap-south-1",
    "ap-southeast-1",
    "ap-southeast-2",
    "ca-central-1",
    "eu-central-1",
    "eu-north-1",
    "eu-south-1",
    "eu-west-1",
    "eu-west-2",
    "eu-west-3",
    "me-south-1",
    "sa-east-1",
    "us-east-1",
    "us-east-2",
    "us-west-1",
    "us-west-2",
]);
var AWS_CN_REGIONS = new Set(["cn-north-1", "cn-northwest-1"]);
var AWS_ISO_REGIONS = new Set(["us-iso-east-1"]);
var AWS_ISO_B_REGIONS = new Set(["us-isob-east-1"]);
var AWS_US_GOV_REGIONS = new Set(["us-gov-east-1", "us-gov-west-1"]);
var defaultRegionInfoProvider = function (region, options) {
    var regionInfo = undefined;
    switch (region) {
        // First, try to match exact region names.
        case "ap-northeast-1":
            regionInfo = {
                hostname: "cognito-identity.ap-northeast-1.amazonaws.com",
                partition: "aws",
            };
            break;
        case "ap-northeast-2":
            regionInfo = {
                hostname: "cognito-identity.ap-northeast-2.amazonaws.com",
                partition: "aws",
            };
            break;
        case "ap-south-1":
            regionInfo = {
                hostname: "cognito-identity.ap-south-1.amazonaws.com",
                partition: "aws",
            };
            break;
        case "ap-southeast-1":
            regionInfo = {
                hostname: "cognito-identity.ap-southeast-1.amazonaws.com",
                partition: "aws",
            };
            break;
        case "ap-southeast-2":
            regionInfo = {
                hostname: "cognito-identity.ap-southeast-2.amazonaws.com",
                partition: "aws",
            };
            break;
        case "ca-central-1":
            regionInfo = {
                hostname: "cognito-identity.ca-central-1.amazonaws.com",
                partition: "aws",
            };
            break;
        case "cn-north-1":
            regionInfo = {
                hostname: "cognito-identity.cn-north-1.amazonaws.com.cn",
                partition: "aws-cn",
            };
            break;
        case "eu-central-1":
            regionInfo = {
                hostname: "cognito-identity.eu-central-1.amazonaws.com",
                partition: "aws",
            };
            break;
        case "eu-north-1":
            regionInfo = {
                hostname: "cognito-identity.eu-north-1.amazonaws.com",
                partition: "aws",
            };
            break;
        case "eu-west-1":
            regionInfo = {
                hostname: "cognito-identity.eu-west-1.amazonaws.com",
                partition: "aws",
            };
            break;
        case "eu-west-2":
            regionInfo = {
                hostname: "cognito-identity.eu-west-2.amazonaws.com",
                partition: "aws",
            };
            break;
        case "eu-west-3":
            regionInfo = {
                hostname: "cognito-identity.eu-west-3.amazonaws.com",
                partition: "aws",
            };
            break;
        case "fips-us-east-1":
            regionInfo = {
                hostname: "cognito-identity-fips.us-east-1.amazonaws.com",
                partition: "aws",
                signingRegion: "us-east-1",
            };
            break;
        case "fips-us-east-2":
            regionInfo = {
                hostname: "cognito-identity-fips.us-east-2.amazonaws.com",
                partition: "aws",
                signingRegion: "us-east-2",
            };
            break;
        case "fips-us-gov-west-1":
            regionInfo = {
                hostname: "cognito-identity-fips.us-gov-west-1.amazonaws.com",
                partition: "aws-us-gov",
                signingRegion: "us-gov-west-1",
            };
            break;
        case "fips-us-west-2":
            regionInfo = {
                hostname: "cognito-identity-fips.us-west-2.amazonaws.com",
                partition: "aws",
                signingRegion: "us-west-2",
            };
            break;
        case "sa-east-1":
            regionInfo = {
                hostname: "cognito-identity.sa-east-1.amazonaws.com",
                partition: "aws",
            };
            break;
        case "us-east-1":
            regionInfo = {
                hostname: "cognito-identity.us-east-1.amazonaws.com",
                partition: "aws",
            };
            break;
        case "us-east-2":
            regionInfo = {
                hostname: "cognito-identity.us-east-2.amazonaws.com",
                partition: "aws",
            };
            break;
        case "us-gov-west-1":
            regionInfo = {
                hostname: "cognito-identity.us-gov-west-1.amazonaws.com",
                partition: "aws-us-gov",
            };
            break;
        case "us-west-1":
            regionInfo = {
                hostname: "cognito-identity.us-west-1.amazonaws.com",
                partition: "aws",
            };
            break;
        case "us-west-2":
            regionInfo = {
                hostname: "cognito-identity.us-west-2.amazonaws.com",
                partition: "aws",
            };
            break;
        // Next, try to match partition endpoints.
        default:
            if (AWS_REGIONS.has(region)) {
                regionInfo = {
                    hostname: AWS_TEMPLATE.replace("{region}", region),
                    partition: "aws",
                };
            }
            if (AWS_CN_REGIONS.has(region)) {
                regionInfo = {
                    hostname: AWS_CN_TEMPLATE.replace("{region}", region),
                    partition: "aws-cn",
                };
            }
            if (AWS_ISO_REGIONS.has(region)) {
                regionInfo = {
                    hostname: AWS_ISO_TEMPLATE.replace("{region}", region),
                    partition: "aws-iso",
                };
            }
            if (AWS_ISO_B_REGIONS.has(region)) {
                regionInfo = {
                    hostname: AWS_ISO_B_TEMPLATE.replace("{region}", region),
                    partition: "aws-iso-b",
                };
            }
            if (AWS_US_GOV_REGIONS.has(region)) {
                regionInfo = {
                    hostname: AWS_US_GOV_TEMPLATE.replace("{region}", region),
                    partition: "aws-us-gov",
                };
            }
            // Finally, assume it's an AWS partition endpoint.
            if (regionInfo === undefined) {
                regionInfo = {
                    hostname: AWS_TEMPLATE.replace("{region}", region),
                    partition: "aws",
                };
            }
    }
    return Promise.resolve(__assign$4({ signingService: "cognito-identity" }, regionInfo));
};

/**
 * @internal
 */
var ClientSharedValues = {
    apiVersion: "2014-06-30",
    disableHostPrefix: false,
    logger: {},
    regionInfoProvider: defaultRegionInfoProvider,
    serviceId: "Cognito Identity",
    urlParser: parseUrl,
};

/**
 * @internal
 */
var ClientDefaultValues = __assign$4(__assign$4({}, ClientSharedValues), { runtime: "browser", base64Decoder: fromBase64, base64Encoder: toBase64, bodyLengthChecker: calculateBodyLength, credentialDefaultProvider: function (_) { return function () { return Promise.reject(new Error("Credential is missing")); }; }, defaultUserAgentProvider: defaultUserAgent({
        serviceId: ClientSharedValues.serviceId,
        clientVersion: packageInfo.version,
    }), maxAttempts: DEFAULT_MAX_ATTEMPTS, region: invalidProvider("Region is missing"), requestHandler: new FetchHttpHandler(), sha256: build.Sha256, streamCollector: streamCollector, utf8Decoder: fromUtf8, utf8Encoder: toUtf8 });

/**
 * <fullname>Amazon Cognito Federated Identities</fullname>
 *          <p>Amazon Cognito Federated Identities is a web service that delivers scoped temporary
 *          credentials to mobile devices and other untrusted environments. It uniquely identifies a
 *          device and supplies the user with a consistent identity over the lifetime of an
 *          application.</p>
 *          <p>Using Amazon Cognito Federated Identities, you can enable authentication with one or
 *          more third-party identity providers (Facebook, Google, or Login with Amazon) or an Amazon
 *          Cognito user pool, and you can also choose to support unauthenticated access from your app.
 *          Cognito delivers a unique identifier for each user and acts as an OpenID token provider
 *          trusted by AWS Security Token Service (STS) to access temporary, limited-privilege AWS
 *          credentials.</p>
 *          <p>For a description of the authentication flow from the Amazon Cognito Developer Guide
 *          see <a href="https://docs.aws.amazon.com/cognito/latest/developerguide/authentication-flow.html">Authentication Flow</a>.</p>
 *          <p>For more information see <a href="https://docs.aws.amazon.com/cognito/latest/developerguide/cognito-identity.html">Amazon Cognito Federated Identities</a>.</p>
 */
var CognitoIdentityClient = /** @class */ (function (_super) {
    __extends$1(CognitoIdentityClient, _super);
    function CognitoIdentityClient(configuration) {
        var _this = this;
        var _config_0 = __assign$4(__assign$4({}, ClientDefaultValues), configuration);
        var _config_1 = resolveRegionConfig(_config_0);
        var _config_2 = resolveEndpointsConfig(_config_1);
        var _config_3 = resolveAwsAuthConfig(_config_2);
        var _config_4 = resolveRetryConfig(_config_3);
        var _config_5 = resolveHostHeaderConfig(_config_4);
        var _config_6 = resolveUserAgentConfig(_config_5);
        _this = _super.call(this, _config_6) || this;
        _this.config = _config_6;
        _this.middlewareStack.use(getRetryPlugin(_this.config));
        _this.middlewareStack.use(getContentLengthPlugin(_this.config));
        _this.middlewareStack.use(getHostHeaderPlugin(_this.config));
        _this.middlewareStack.use(getLoggerPlugin(_this.config));
        _this.middlewareStack.use(getUserAgentPlugin(_this.config));
        return _this;
    }
    CognitoIdentityClient.prototype.destroy = function () {
        _super.prototype.destroy.call(this);
    };
    return CognitoIdentityClient;
}(Client$1));

var AmbiguousRoleResolutionType;
(function (AmbiguousRoleResolutionType) {
    AmbiguousRoleResolutionType["AUTHENTICATED_ROLE"] = "AuthenticatedRole";
    AmbiguousRoleResolutionType["DENY"] = "Deny";
})(AmbiguousRoleResolutionType || (AmbiguousRoleResolutionType = {}));
var CognitoIdentityProvider;
(function (CognitoIdentityProvider) {
    CognitoIdentityProvider.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(CognitoIdentityProvider || (CognitoIdentityProvider = {}));
var CreateIdentityPoolInput;
(function (CreateIdentityPoolInput) {
    CreateIdentityPoolInput.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(CreateIdentityPoolInput || (CreateIdentityPoolInput = {}));
var IdentityPool;
(function (IdentityPool) {
    IdentityPool.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(IdentityPool || (IdentityPool = {}));
var InternalErrorException;
(function (InternalErrorException) {
    InternalErrorException.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(InternalErrorException || (InternalErrorException = {}));
var InvalidParameterException;
(function (InvalidParameterException) {
    InvalidParameterException.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(InvalidParameterException || (InvalidParameterException = {}));
var LimitExceededException;
(function (LimitExceededException) {
    LimitExceededException.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(LimitExceededException || (LimitExceededException = {}));
var NotAuthorizedException;
(function (NotAuthorizedException) {
    NotAuthorizedException.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(NotAuthorizedException || (NotAuthorizedException = {}));
var ResourceConflictException;
(function (ResourceConflictException) {
    ResourceConflictException.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(ResourceConflictException || (ResourceConflictException = {}));
var TooManyRequestsException;
(function (TooManyRequestsException) {
    TooManyRequestsException.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(TooManyRequestsException || (TooManyRequestsException = {}));
var DeleteIdentitiesInput;
(function (DeleteIdentitiesInput) {
    DeleteIdentitiesInput.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(DeleteIdentitiesInput || (DeleteIdentitiesInput = {}));
var ErrorCode;
(function (ErrorCode) {
    ErrorCode["ACCESS_DENIED"] = "AccessDenied";
    ErrorCode["INTERNAL_SERVER_ERROR"] = "InternalServerError";
})(ErrorCode || (ErrorCode = {}));
var UnprocessedIdentityId;
(function (UnprocessedIdentityId) {
    UnprocessedIdentityId.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(UnprocessedIdentityId || (UnprocessedIdentityId = {}));
var DeleteIdentitiesResponse;
(function (DeleteIdentitiesResponse) {
    DeleteIdentitiesResponse.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(DeleteIdentitiesResponse || (DeleteIdentitiesResponse = {}));
var DeleteIdentityPoolInput;
(function (DeleteIdentityPoolInput) {
    DeleteIdentityPoolInput.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(DeleteIdentityPoolInput || (DeleteIdentityPoolInput = {}));
var ResourceNotFoundException;
(function (ResourceNotFoundException) {
    ResourceNotFoundException.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(ResourceNotFoundException || (ResourceNotFoundException = {}));
var DescribeIdentityInput;
(function (DescribeIdentityInput) {
    DescribeIdentityInput.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(DescribeIdentityInput || (DescribeIdentityInput = {}));
var IdentityDescription;
(function (IdentityDescription) {
    IdentityDescription.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(IdentityDescription || (IdentityDescription = {}));
var DescribeIdentityPoolInput;
(function (DescribeIdentityPoolInput) {
    DescribeIdentityPoolInput.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(DescribeIdentityPoolInput || (DescribeIdentityPoolInput = {}));
var ExternalServiceException;
(function (ExternalServiceException) {
    ExternalServiceException.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(ExternalServiceException || (ExternalServiceException = {}));
var GetCredentialsForIdentityInput;
(function (GetCredentialsForIdentityInput) {
    GetCredentialsForIdentityInput.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(GetCredentialsForIdentityInput || (GetCredentialsForIdentityInput = {}));
var Credentials$1;
(function (Credentials) {
    Credentials.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(Credentials$1 || (Credentials$1 = {}));
var GetCredentialsForIdentityResponse;
(function (GetCredentialsForIdentityResponse) {
    GetCredentialsForIdentityResponse.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(GetCredentialsForIdentityResponse || (GetCredentialsForIdentityResponse = {}));
var InvalidIdentityPoolConfigurationException;
(function (InvalidIdentityPoolConfigurationException) {
    InvalidIdentityPoolConfigurationException.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(InvalidIdentityPoolConfigurationException || (InvalidIdentityPoolConfigurationException = {}));
var GetIdInput;
(function (GetIdInput) {
    GetIdInput.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(GetIdInput || (GetIdInput = {}));
var GetIdResponse;
(function (GetIdResponse) {
    GetIdResponse.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(GetIdResponse || (GetIdResponse = {}));
var GetIdentityPoolRolesInput;
(function (GetIdentityPoolRolesInput) {
    GetIdentityPoolRolesInput.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(GetIdentityPoolRolesInput || (GetIdentityPoolRolesInput = {}));
var MappingRuleMatchType;
(function (MappingRuleMatchType) {
    MappingRuleMatchType["CONTAINS"] = "Contains";
    MappingRuleMatchType["EQUALS"] = "Equals";
    MappingRuleMatchType["NOT_EQUAL"] = "NotEqual";
    MappingRuleMatchType["STARTS_WITH"] = "StartsWith";
})(MappingRuleMatchType || (MappingRuleMatchType = {}));
var MappingRule;
(function (MappingRule) {
    MappingRule.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(MappingRule || (MappingRule = {}));
var RulesConfigurationType;
(function (RulesConfigurationType) {
    RulesConfigurationType.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(RulesConfigurationType || (RulesConfigurationType = {}));
var RoleMappingType;
(function (RoleMappingType) {
    RoleMappingType["RULES"] = "Rules";
    RoleMappingType["TOKEN"] = "Token";
})(RoleMappingType || (RoleMappingType = {}));
var RoleMapping;
(function (RoleMapping) {
    RoleMapping.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(RoleMapping || (RoleMapping = {}));
var GetIdentityPoolRolesResponse;
(function (GetIdentityPoolRolesResponse) {
    GetIdentityPoolRolesResponse.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(GetIdentityPoolRolesResponse || (GetIdentityPoolRolesResponse = {}));
var GetOpenIdTokenInput;
(function (GetOpenIdTokenInput) {
    GetOpenIdTokenInput.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(GetOpenIdTokenInput || (GetOpenIdTokenInput = {}));
var GetOpenIdTokenResponse;
(function (GetOpenIdTokenResponse) {
    GetOpenIdTokenResponse.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(GetOpenIdTokenResponse || (GetOpenIdTokenResponse = {}));
var DeveloperUserAlreadyRegisteredException;
(function (DeveloperUserAlreadyRegisteredException) {
    DeveloperUserAlreadyRegisteredException.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(DeveloperUserAlreadyRegisteredException || (DeveloperUserAlreadyRegisteredException = {}));
var GetOpenIdTokenForDeveloperIdentityInput;
(function (GetOpenIdTokenForDeveloperIdentityInput) {
    GetOpenIdTokenForDeveloperIdentityInput.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(GetOpenIdTokenForDeveloperIdentityInput || (GetOpenIdTokenForDeveloperIdentityInput = {}));
var GetOpenIdTokenForDeveloperIdentityResponse;
(function (GetOpenIdTokenForDeveloperIdentityResponse) {
    GetOpenIdTokenForDeveloperIdentityResponse.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(GetOpenIdTokenForDeveloperIdentityResponse || (GetOpenIdTokenForDeveloperIdentityResponse = {}));
var ListIdentitiesInput;
(function (ListIdentitiesInput) {
    ListIdentitiesInput.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(ListIdentitiesInput || (ListIdentitiesInput = {}));
var ListIdentitiesResponse;
(function (ListIdentitiesResponse) {
    ListIdentitiesResponse.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(ListIdentitiesResponse || (ListIdentitiesResponse = {}));
var ListIdentityPoolsInput;
(function (ListIdentityPoolsInput) {
    ListIdentityPoolsInput.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(ListIdentityPoolsInput || (ListIdentityPoolsInput = {}));
var IdentityPoolShortDescription;
(function (IdentityPoolShortDescription) {
    IdentityPoolShortDescription.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(IdentityPoolShortDescription || (IdentityPoolShortDescription = {}));
var ListIdentityPoolsResponse;
(function (ListIdentityPoolsResponse) {
    ListIdentityPoolsResponse.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(ListIdentityPoolsResponse || (ListIdentityPoolsResponse = {}));
var ListTagsForResourceInput;
(function (ListTagsForResourceInput) {
    ListTagsForResourceInput.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(ListTagsForResourceInput || (ListTagsForResourceInput = {}));
var ListTagsForResourceResponse;
(function (ListTagsForResourceResponse) {
    ListTagsForResourceResponse.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(ListTagsForResourceResponse || (ListTagsForResourceResponse = {}));
var LookupDeveloperIdentityInput;
(function (LookupDeveloperIdentityInput) {
    LookupDeveloperIdentityInput.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(LookupDeveloperIdentityInput || (LookupDeveloperIdentityInput = {}));
var LookupDeveloperIdentityResponse;
(function (LookupDeveloperIdentityResponse) {
    LookupDeveloperIdentityResponse.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(LookupDeveloperIdentityResponse || (LookupDeveloperIdentityResponse = {}));
var MergeDeveloperIdentitiesInput;
(function (MergeDeveloperIdentitiesInput) {
    MergeDeveloperIdentitiesInput.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(MergeDeveloperIdentitiesInput || (MergeDeveloperIdentitiesInput = {}));
var MergeDeveloperIdentitiesResponse;
(function (MergeDeveloperIdentitiesResponse) {
    MergeDeveloperIdentitiesResponse.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(MergeDeveloperIdentitiesResponse || (MergeDeveloperIdentitiesResponse = {}));
var ConcurrentModificationException;
(function (ConcurrentModificationException) {
    ConcurrentModificationException.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(ConcurrentModificationException || (ConcurrentModificationException = {}));
var SetIdentityPoolRolesInput;
(function (SetIdentityPoolRolesInput) {
    SetIdentityPoolRolesInput.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(SetIdentityPoolRolesInput || (SetIdentityPoolRolesInput = {}));
var TagResourceInput;
(function (TagResourceInput) {
    TagResourceInput.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(TagResourceInput || (TagResourceInput = {}));
var TagResourceResponse;
(function (TagResourceResponse) {
    TagResourceResponse.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(TagResourceResponse || (TagResourceResponse = {}));
var UnlinkDeveloperIdentityInput;
(function (UnlinkDeveloperIdentityInput) {
    UnlinkDeveloperIdentityInput.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(UnlinkDeveloperIdentityInput || (UnlinkDeveloperIdentityInput = {}));
var UnlinkIdentityInput;
(function (UnlinkIdentityInput) {
    UnlinkIdentityInput.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(UnlinkIdentityInput || (UnlinkIdentityInput = {}));
var UntagResourceInput;
(function (UntagResourceInput) {
    UntagResourceInput.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(UntagResourceInput || (UntagResourceInput = {}));
var UntagResourceResponse;
(function (UntagResourceResponse) {
    UntagResourceResponse.filterSensitiveLog = function (obj) { return (__assign$4({}, obj)); };
})(UntagResourceResponse || (UntagResourceResponse = {}));

var serializeAws_json1_1GetCredentialsForIdentityCommand = function (input, context) { return __awaiter$3(void 0, void 0, void 0, function () {
    var headers, body;
    return __generator$3(this, function (_a) {
        headers = {
            "content-type": "application/x-amz-json-1.1",
            "x-amz-target": "AWSCognitoIdentityService.GetCredentialsForIdentity",
        };
        body = JSON.stringify(serializeAws_json1_1GetCredentialsForIdentityInput(input));
        return [2 /*return*/, buildHttpRpcRequest(context, headers, "/", undefined, body)];
    });
}); };
var serializeAws_json1_1GetIdCommand = function (input, context) { return __awaiter$3(void 0, void 0, void 0, function () {
    var headers, body;
    return __generator$3(this, function (_a) {
        headers = {
            "content-type": "application/x-amz-json-1.1",
            "x-amz-target": "AWSCognitoIdentityService.GetId",
        };
        body = JSON.stringify(serializeAws_json1_1GetIdInput(input));
        return [2 /*return*/, buildHttpRpcRequest(context, headers, "/", undefined, body)];
    });
}); };
var deserializeAws_json1_1GetCredentialsForIdentityCommand = function (output, context) { return __awaiter$3(void 0, void 0, void 0, function () {
    var data, contents, response;
    return __generator$3(this, function (_a) {
        switch (_a.label) {
            case 0:
                if (output.statusCode >= 300) {
                    return [2 /*return*/, deserializeAws_json1_1GetCredentialsForIdentityCommandError(output, context)];
                }
                return [4 /*yield*/, parseBody(output.body, context)];
            case 1:
                data = _a.sent();
                contents = {};
                contents = deserializeAws_json1_1GetCredentialsForIdentityResponse(data);
                response = __assign$4({ $metadata: deserializeMetadata(output) }, contents);
                return [2 /*return*/, Promise.resolve(response)];
        }
    });
}); };
var deserializeAws_json1_1GetCredentialsForIdentityCommandError = function (output, context) { return __awaiter$3(void 0, void 0, void 0, function () {
    var parsedOutput, _a, response, errorCode, _b, _c, _d, _e, _f, _g, _h, _j, _k, parsedBody, message;
    var _l;
    return __generator$3(this, function (_m) {
        switch (_m.label) {
            case 0:
                _a = [__assign$4({}, output)];
                _l = {};
                return [4 /*yield*/, parseBody(output.body, context)];
            case 1:
                parsedOutput = __assign$4.apply(void 0, _a.concat([(_l.body = _m.sent(), _l)]));
                errorCode = "UnknownError";
                errorCode = loadRestJsonErrorCode(output, parsedOutput.body);
                _b = errorCode;
                switch (_b) {
                    case "ExternalServiceException": return [3 /*break*/, 2];
                    case "com.amazonaws.cognitoidentity#ExternalServiceException": return [3 /*break*/, 2];
                    case "InternalErrorException": return [3 /*break*/, 4];
                    case "com.amazonaws.cognitoidentity#InternalErrorException": return [3 /*break*/, 4];
                    case "InvalidIdentityPoolConfigurationException": return [3 /*break*/, 6];
                    case "com.amazonaws.cognitoidentity#InvalidIdentityPoolConfigurationException": return [3 /*break*/, 6];
                    case "InvalidParameterException": return [3 /*break*/, 8];
                    case "com.amazonaws.cognitoidentity#InvalidParameterException": return [3 /*break*/, 8];
                    case "NotAuthorizedException": return [3 /*break*/, 10];
                    case "com.amazonaws.cognitoidentity#NotAuthorizedException": return [3 /*break*/, 10];
                    case "ResourceConflictException": return [3 /*break*/, 12];
                    case "com.amazonaws.cognitoidentity#ResourceConflictException": return [3 /*break*/, 12];
                    case "ResourceNotFoundException": return [3 /*break*/, 14];
                    case "com.amazonaws.cognitoidentity#ResourceNotFoundException": return [3 /*break*/, 14];
                    case "TooManyRequestsException": return [3 /*break*/, 16];
                    case "com.amazonaws.cognitoidentity#TooManyRequestsException": return [3 /*break*/, 16];
                }
                return [3 /*break*/, 18];
            case 2:
                _c = [{}];
                return [4 /*yield*/, deserializeAws_json1_1ExternalServiceExceptionResponse(parsedOutput)];
            case 3:
                response = __assign$4.apply(void 0, [__assign$4.apply(void 0, _c.concat([(_m.sent())])), { name: errorCode, $metadata: deserializeMetadata(output) }]);
                return [3 /*break*/, 19];
            case 4:
                _d = [{}];
                return [4 /*yield*/, deserializeAws_json1_1InternalErrorExceptionResponse(parsedOutput)];
            case 5:
                response = __assign$4.apply(void 0, [__assign$4.apply(void 0, _d.concat([(_m.sent())])), { name: errorCode, $metadata: deserializeMetadata(output) }]);
                return [3 /*break*/, 19];
            case 6:
                _e = [{}];
                return [4 /*yield*/, deserializeAws_json1_1InvalidIdentityPoolConfigurationExceptionResponse(parsedOutput)];
            case 7:
                response = __assign$4.apply(void 0, [__assign$4.apply(void 0, _e.concat([(_m.sent())])), { name: errorCode, $metadata: deserializeMetadata(output) }]);
                return [3 /*break*/, 19];
            case 8:
                _f = [{}];
                return [4 /*yield*/, deserializeAws_json1_1InvalidParameterExceptionResponse(parsedOutput)];
            case 9:
                response = __assign$4.apply(void 0, [__assign$4.apply(void 0, _f.concat([(_m.sent())])), { name: errorCode, $metadata: deserializeMetadata(output) }]);
                return [3 /*break*/, 19];
            case 10:
                _g = [{}];
                return [4 /*yield*/, deserializeAws_json1_1NotAuthorizedExceptionResponse(parsedOutput)];
            case 11:
                response = __assign$4.apply(void 0, [__assign$4.apply(void 0, _g.concat([(_m.sent())])), { name: errorCode, $metadata: deserializeMetadata(output) }]);
                return [3 /*break*/, 19];
            case 12:
                _h = [{}];
                return [4 /*yield*/, deserializeAws_json1_1ResourceConflictExceptionResponse(parsedOutput)];
            case 13:
                response = __assign$4.apply(void 0, [__assign$4.apply(void 0, _h.concat([(_m.sent())])), { name: errorCode, $metadata: deserializeMetadata(output) }]);
                return [3 /*break*/, 19];
            case 14:
                _j = [{}];
                return [4 /*yield*/, deserializeAws_json1_1ResourceNotFoundExceptionResponse(parsedOutput)];
            case 15:
                response = __assign$4.apply(void 0, [__assign$4.apply(void 0, _j.concat([(_m.sent())])), { name: errorCode, $metadata: deserializeMetadata(output) }]);
                return [3 /*break*/, 19];
            case 16:
                _k = [{}];
                return [4 /*yield*/, deserializeAws_json1_1TooManyRequestsExceptionResponse(parsedOutput)];
            case 17:
                response = __assign$4.apply(void 0, [__assign$4.apply(void 0, _k.concat([(_m.sent())])), { name: errorCode, $metadata: deserializeMetadata(output) }]);
                return [3 /*break*/, 19];
            case 18:
                parsedBody = parsedOutput.body;
                errorCode = parsedBody.code || parsedBody.Code || errorCode;
                response = __assign$4(__assign$4({}, parsedBody), { name: "" + errorCode, message: parsedBody.message || parsedBody.Message || errorCode, $fault: "client", $metadata: deserializeMetadata(output) });
                _m.label = 19;
            case 19:
                message = response.message || response.Message || errorCode;
                response.message = message;
                delete response.Message;
                return [2 /*return*/, Promise.reject(Object.assign(new Error(message), response))];
        }
    });
}); };
var deserializeAws_json1_1GetIdCommand = function (output, context) { return __awaiter$3(void 0, void 0, void 0, function () {
    var data, contents, response;
    return __generator$3(this, function (_a) {
        switch (_a.label) {
            case 0:
                if (output.statusCode >= 300) {
                    return [2 /*return*/, deserializeAws_json1_1GetIdCommandError(output, context)];
                }
                return [4 /*yield*/, parseBody(output.body, context)];
            case 1:
                data = _a.sent();
                contents = {};
                contents = deserializeAws_json1_1GetIdResponse(data);
                response = __assign$4({ $metadata: deserializeMetadata(output) }, contents);
                return [2 /*return*/, Promise.resolve(response)];
        }
    });
}); };
var deserializeAws_json1_1GetIdCommandError = function (output, context) { return __awaiter$3(void 0, void 0, void 0, function () {
    var parsedOutput, _a, response, errorCode, _b, _c, _d, _e, _f, _g, _h, _j, _k, parsedBody, message;
    var _l;
    return __generator$3(this, function (_m) {
        switch (_m.label) {
            case 0:
                _a = [__assign$4({}, output)];
                _l = {};
                return [4 /*yield*/, parseBody(output.body, context)];
            case 1:
                parsedOutput = __assign$4.apply(void 0, _a.concat([(_l.body = _m.sent(), _l)]));
                errorCode = "UnknownError";
                errorCode = loadRestJsonErrorCode(output, parsedOutput.body);
                _b = errorCode;
                switch (_b) {
                    case "ExternalServiceException": return [3 /*break*/, 2];
                    case "com.amazonaws.cognitoidentity#ExternalServiceException": return [3 /*break*/, 2];
                    case "InternalErrorException": return [3 /*break*/, 4];
                    case "com.amazonaws.cognitoidentity#InternalErrorException": return [3 /*break*/, 4];
                    case "InvalidParameterException": return [3 /*break*/, 6];
                    case "com.amazonaws.cognitoidentity#InvalidParameterException": return [3 /*break*/, 6];
                    case "LimitExceededException": return [3 /*break*/, 8];
                    case "com.amazonaws.cognitoidentity#LimitExceededException": return [3 /*break*/, 8];
                    case "NotAuthorizedException": return [3 /*break*/, 10];
                    case "com.amazonaws.cognitoidentity#NotAuthorizedException": return [3 /*break*/, 10];
                    case "ResourceConflictException": return [3 /*break*/, 12];
                    case "com.amazonaws.cognitoidentity#ResourceConflictException": return [3 /*break*/, 12];
                    case "ResourceNotFoundException": return [3 /*break*/, 14];
                    case "com.amazonaws.cognitoidentity#ResourceNotFoundException": return [3 /*break*/, 14];
                    case "TooManyRequestsException": return [3 /*break*/, 16];
                    case "com.amazonaws.cognitoidentity#TooManyRequestsException": return [3 /*break*/, 16];
                }
                return [3 /*break*/, 18];
            case 2:
                _c = [{}];
                return [4 /*yield*/, deserializeAws_json1_1ExternalServiceExceptionResponse(parsedOutput)];
            case 3:
                response = __assign$4.apply(void 0, [__assign$4.apply(void 0, _c.concat([(_m.sent())])), { name: errorCode, $metadata: deserializeMetadata(output) }]);
                return [3 /*break*/, 19];
            case 4:
                _d = [{}];
                return [4 /*yield*/, deserializeAws_json1_1InternalErrorExceptionResponse(parsedOutput)];
            case 5:
                response = __assign$4.apply(void 0, [__assign$4.apply(void 0, _d.concat([(_m.sent())])), { name: errorCode, $metadata: deserializeMetadata(output) }]);
                return [3 /*break*/, 19];
            case 6:
                _e = [{}];
                return [4 /*yield*/, deserializeAws_json1_1InvalidParameterExceptionResponse(parsedOutput)];
            case 7:
                response = __assign$4.apply(void 0, [__assign$4.apply(void 0, _e.concat([(_m.sent())])), { name: errorCode, $metadata: deserializeMetadata(output) }]);
                return [3 /*break*/, 19];
            case 8:
                _f = [{}];
                return [4 /*yield*/, deserializeAws_json1_1LimitExceededExceptionResponse(parsedOutput)];
            case 9:
                response = __assign$4.apply(void 0, [__assign$4.apply(void 0, _f.concat([(_m.sent())])), { name: errorCode, $metadata: deserializeMetadata(output) }]);
                return [3 /*break*/, 19];
            case 10:
                _g = [{}];
                return [4 /*yield*/, deserializeAws_json1_1NotAuthorizedExceptionResponse(parsedOutput)];
            case 11:
                response = __assign$4.apply(void 0, [__assign$4.apply(void 0, _g.concat([(_m.sent())])), { name: errorCode, $metadata: deserializeMetadata(output) }]);
                return [3 /*break*/, 19];
            case 12:
                _h = [{}];
                return [4 /*yield*/, deserializeAws_json1_1ResourceConflictExceptionResponse(parsedOutput)];
            case 13:
                response = __assign$4.apply(void 0, [__assign$4.apply(void 0, _h.concat([(_m.sent())])), { name: errorCode, $metadata: deserializeMetadata(output) }]);
                return [3 /*break*/, 19];
            case 14:
                _j = [{}];
                return [4 /*yield*/, deserializeAws_json1_1ResourceNotFoundExceptionResponse(parsedOutput)];
            case 15:
                response = __assign$4.apply(void 0, [__assign$4.apply(void 0, _j.concat([(_m.sent())])), { name: errorCode, $metadata: deserializeMetadata(output) }]);
                return [3 /*break*/, 19];
            case 16:
                _k = [{}];
                return [4 /*yield*/, deserializeAws_json1_1TooManyRequestsExceptionResponse(parsedOutput)];
            case 17:
                response = __assign$4.apply(void 0, [__assign$4.apply(void 0, _k.concat([(_m.sent())])), { name: errorCode, $metadata: deserializeMetadata(output) }]);
                return [3 /*break*/, 19];
            case 18:
                parsedBody = parsedOutput.body;
                errorCode = parsedBody.code || parsedBody.Code || errorCode;
                response = __assign$4(__assign$4({}, parsedBody), { name: "" + errorCode, message: parsedBody.message || parsedBody.Message || errorCode, $fault: "client", $metadata: deserializeMetadata(output) });
                _m.label = 19;
            case 19:
                message = response.message || response.Message || errorCode;
                response.message = message;
                delete response.Message;
                return [2 /*return*/, Promise.reject(Object.assign(new Error(message), response))];
        }
    });
}); };
var deserializeAws_json1_1ExternalServiceExceptionResponse = function (parsedOutput, context) { return __awaiter$3(void 0, void 0, void 0, function () {
    var body, deserialized, contents;
    return __generator$3(this, function (_a) {
        body = parsedOutput.body;
        deserialized = deserializeAws_json1_1ExternalServiceException(body);
        contents = __assign$4({ name: "ExternalServiceException", $fault: "client", $metadata: deserializeMetadata(parsedOutput) }, deserialized);
        return [2 /*return*/, contents];
    });
}); };
var deserializeAws_json1_1InternalErrorExceptionResponse = function (parsedOutput, context) { return __awaiter$3(void 0, void 0, void 0, function () {
    var body, deserialized, contents;
    return __generator$3(this, function (_a) {
        body = parsedOutput.body;
        deserialized = deserializeAws_json1_1InternalErrorException(body);
        contents = __assign$4({ name: "InternalErrorException", $fault: "server", $metadata: deserializeMetadata(parsedOutput) }, deserialized);
        return [2 /*return*/, contents];
    });
}); };
var deserializeAws_json1_1InvalidIdentityPoolConfigurationExceptionResponse = function (parsedOutput, context) { return __awaiter$3(void 0, void 0, void 0, function () {
    var body, deserialized, contents;
    return __generator$3(this, function (_a) {
        body = parsedOutput.body;
        deserialized = deserializeAws_json1_1InvalidIdentityPoolConfigurationException(body);
        contents = __assign$4({ name: "InvalidIdentityPoolConfigurationException", $fault: "client", $metadata: deserializeMetadata(parsedOutput) }, deserialized);
        return [2 /*return*/, contents];
    });
}); };
var deserializeAws_json1_1InvalidParameterExceptionResponse = function (parsedOutput, context) { return __awaiter$3(void 0, void 0, void 0, function () {
    var body, deserialized, contents;
    return __generator$3(this, function (_a) {
        body = parsedOutput.body;
        deserialized = deserializeAws_json1_1InvalidParameterException(body);
        contents = __assign$4({ name: "InvalidParameterException", $fault: "client", $metadata: deserializeMetadata(parsedOutput) }, deserialized);
        return [2 /*return*/, contents];
    });
}); };
var deserializeAws_json1_1LimitExceededExceptionResponse = function (parsedOutput, context) { return __awaiter$3(void 0, void 0, void 0, function () {
    var body, deserialized, contents;
    return __generator$3(this, function (_a) {
        body = parsedOutput.body;
        deserialized = deserializeAws_json1_1LimitExceededException(body);
        contents = __assign$4({ name: "LimitExceededException", $fault: "client", $metadata: deserializeMetadata(parsedOutput) }, deserialized);
        return [2 /*return*/, contents];
    });
}); };
var deserializeAws_json1_1NotAuthorizedExceptionResponse = function (parsedOutput, context) { return __awaiter$3(void 0, void 0, void 0, function () {
    var body, deserialized, contents;
    return __generator$3(this, function (_a) {
        body = parsedOutput.body;
        deserialized = deserializeAws_json1_1NotAuthorizedException(body);
        contents = __assign$4({ name: "NotAuthorizedException", $fault: "client", $metadata: deserializeMetadata(parsedOutput) }, deserialized);
        return [2 /*return*/, contents];
    });
}); };
var deserializeAws_json1_1ResourceConflictExceptionResponse = function (parsedOutput, context) { return __awaiter$3(void 0, void 0, void 0, function () {
    var body, deserialized, contents;
    return __generator$3(this, function (_a) {
        body = parsedOutput.body;
        deserialized = deserializeAws_json1_1ResourceConflictException(body);
        contents = __assign$4({ name: "ResourceConflictException", $fault: "client", $metadata: deserializeMetadata(parsedOutput) }, deserialized);
        return [2 /*return*/, contents];
    });
}); };
var deserializeAws_json1_1ResourceNotFoundExceptionResponse = function (parsedOutput, context) { return __awaiter$3(void 0, void 0, void 0, function () {
    var body, deserialized, contents;
    return __generator$3(this, function (_a) {
        body = parsedOutput.body;
        deserialized = deserializeAws_json1_1ResourceNotFoundException(body);
        contents = __assign$4({ name: "ResourceNotFoundException", $fault: "client", $metadata: deserializeMetadata(parsedOutput) }, deserialized);
        return [2 /*return*/, contents];
    });
}); };
var deserializeAws_json1_1TooManyRequestsExceptionResponse = function (parsedOutput, context) { return __awaiter$3(void 0, void 0, void 0, function () {
    var body, deserialized, contents;
    return __generator$3(this, function (_a) {
        body = parsedOutput.body;
        deserialized = deserializeAws_json1_1TooManyRequestsException(body);
        contents = __assign$4({ name: "TooManyRequestsException", $fault: "client", $metadata: deserializeMetadata(parsedOutput) }, deserialized);
        return [2 /*return*/, contents];
    });
}); };
var serializeAws_json1_1GetCredentialsForIdentityInput = function (input, context) {
    return __assign$4(__assign$4(__assign$4({}, (input.CustomRoleArn !== undefined && input.CustomRoleArn !== null && { CustomRoleArn: input.CustomRoleArn })), (input.IdentityId !== undefined && input.IdentityId !== null && { IdentityId: input.IdentityId })), (input.Logins !== undefined &&
        input.Logins !== null && { Logins: serializeAws_json1_1LoginsMap(input.Logins) }));
};
var serializeAws_json1_1GetIdInput = function (input, context) {
    return __assign$4(__assign$4(__assign$4({}, (input.AccountId !== undefined && input.AccountId !== null && { AccountId: input.AccountId })), (input.IdentityPoolId !== undefined &&
        input.IdentityPoolId !== null && { IdentityPoolId: input.IdentityPoolId })), (input.Logins !== undefined &&
        input.Logins !== null && { Logins: serializeAws_json1_1LoginsMap(input.Logins) }));
};
var serializeAws_json1_1LoginsMap = function (input, context) {
    return Object.entries(input).reduce(function (acc, _a) {
        var _b;
        var _c = __read$2(_a, 2), key = _c[0], value = _c[1];
        if (value === null) {
            return acc;
        }
        return __assign$4(__assign$4({}, acc), (_b = {}, _b[key] = value, _b));
    }, {});
};
var deserializeAws_json1_1Credentials = function (output, context) {
    return {
        AccessKeyId: output.AccessKeyId !== undefined && output.AccessKeyId !== null ? output.AccessKeyId : undefined,
        Expiration: output.Expiration !== undefined && output.Expiration !== null
            ? new Date(Math.round(output.Expiration * 1000))
            : undefined,
        SecretKey: output.SecretKey !== undefined && output.SecretKey !== null ? output.SecretKey : undefined,
        SessionToken: output.SessionToken !== undefined && output.SessionToken !== null ? output.SessionToken : undefined,
    };
};
var deserializeAws_json1_1ExternalServiceException = function (output, context) {
    return {
        message: output.message !== undefined && output.message !== null ? output.message : undefined,
    };
};
var deserializeAws_json1_1GetCredentialsForIdentityResponse = function (output, context) {
    return {
        Credentials: output.Credentials !== undefined && output.Credentials !== null
            ? deserializeAws_json1_1Credentials(output.Credentials)
            : undefined,
        IdentityId: output.IdentityId !== undefined && output.IdentityId !== null ? output.IdentityId : undefined,
    };
};
var deserializeAws_json1_1GetIdResponse = function (output, context) {
    return {
        IdentityId: output.IdentityId !== undefined && output.IdentityId !== null ? output.IdentityId : undefined,
    };
};
var deserializeAws_json1_1InternalErrorException = function (output, context) {
    return {
        message: output.message !== undefined && output.message !== null ? output.message : undefined,
    };
};
var deserializeAws_json1_1InvalidIdentityPoolConfigurationException = function (output, context) {
    return {
        message: output.message !== undefined && output.message !== null ? output.message : undefined,
    };
};
var deserializeAws_json1_1InvalidParameterException = function (output, context) {
    return {
        message: output.message !== undefined && output.message !== null ? output.message : undefined,
    };
};
var deserializeAws_json1_1LimitExceededException = function (output, context) {
    return {
        message: output.message !== undefined && output.message !== null ? output.message : undefined,
    };
};
var deserializeAws_json1_1NotAuthorizedException = function (output, context) {
    return {
        message: output.message !== undefined && output.message !== null ? output.message : undefined,
    };
};
var deserializeAws_json1_1ResourceConflictException = function (output, context) {
    return {
        message: output.message !== undefined && output.message !== null ? output.message : undefined,
    };
};
var deserializeAws_json1_1ResourceNotFoundException = function (output, context) {
    return {
        message: output.message !== undefined && output.message !== null ? output.message : undefined,
    };
};
var deserializeAws_json1_1TooManyRequestsException = function (output, context) {
    return {
        message: output.message !== undefined && output.message !== null ? output.message : undefined,
    };
};
var deserializeMetadata = function (output) {
    var _a;
    return ({
        httpStatusCode: output.statusCode,
        requestId: (_a = output.headers["x-amzn-requestid"]) !== null && _a !== void 0 ? _a : output.headers["x-amzn-request-id"],
        extendedRequestId: output.headers["x-amz-id-2"],
        cfId: output.headers["x-amz-cf-id"],
    });
};
// Collect low-level response body stream to Uint8Array.
var collectBody = function (streamBody, context) {
    if (streamBody === void 0) { streamBody = new Uint8Array(); }
    if (streamBody instanceof Uint8Array) {
        return Promise.resolve(streamBody);
    }
    return context.streamCollector(streamBody) || Promise.resolve(new Uint8Array());
};
// Encode Uint8Array data into string with utf-8.
var collectBodyString = function (streamBody, context) {
    return collectBody(streamBody, context).then(function (body) { return context.utf8Encoder(body); });
};
var buildHttpRpcRequest = function (context, headers, path, resolvedHostname, body) { return __awaiter$3(void 0, void 0, void 0, function () {
    var _a, hostname, _b, protocol, port, contents;
    return __generator$3(this, function (_c) {
        switch (_c.label) {
            case 0: return [4 /*yield*/, context.endpoint()];
            case 1:
                _a = _c.sent(), hostname = _a.hostname, _b = _a.protocol, protocol = _b === void 0 ? "https" : _b, port = _a.port;
                contents = {
                    protocol: protocol,
                    hostname: hostname,
                    port: port,
                    method: "POST",
                    path: path,
                    headers: headers,
                };
                if (resolvedHostname !== undefined) {
                    contents.hostname = resolvedHostname;
                }
                if (body !== undefined) {
                    contents.body = body;
                }
                return [2 /*return*/, new HttpRequest(contents)];
        }
    });
}); };
var parseBody = function (streamBody, context) {
    return collectBodyString(streamBody, context).then(function (encoded) {
        if (encoded.length) {
            return JSON.parse(encoded);
        }
        return {};
    });
};
/**
 * Load an error code for the aws.rest-json-1.1 protocol.
 */
var loadRestJsonErrorCode = function (output, data) {
    var findKey = function (object, key) { return Object.keys(object).find(function (k) { return k.toLowerCase() === key.toLowerCase(); }); };
    var sanitizeErrorCode = function (rawValue) {
        var cleanValue = rawValue;
        if (cleanValue.indexOf(":") >= 0) {
            cleanValue = cleanValue.split(":")[0];
        }
        if (cleanValue.indexOf("#") >= 0) {
            cleanValue = cleanValue.split("#")[1];
        }
        return cleanValue;
    };
    var headerKey = findKey(output.headers, "x-amzn-errortype");
    if (headerKey !== undefined) {
        return sanitizeErrorCode(output.headers[headerKey]);
    }
    if (data.code !== undefined) {
        return sanitizeErrorCode(data.code);
    }
    if (data["__type"] !== undefined) {
        return sanitizeErrorCode(data["__type"]);
    }
    return "";
};

/**
 * <p>Returns credentials for the provided identity ID. Any provided logins will be
 *          validated against supported login providers. If the token is for
 *          cognito-identity.amazonaws.com, it will be passed through to AWS Security Token Service
 *          with the appropriate role for the token.</p>
 *          <p>This is a public API. You do not need any credentials to call this API.</p>
 */
var GetCredentialsForIdentityCommand = /** @class */ (function (_super) {
    __extends$1(GetCredentialsForIdentityCommand, _super);
    // Start section: command_properties
    // End section: command_properties
    function GetCredentialsForIdentityCommand(input) {
        var _this = 
        // Start section: command_constructor
        _super.call(this) || this;
        _this.input = input;
        return _this;
        // End section: command_constructor
    }
    /**
     * @internal
     */
    GetCredentialsForIdentityCommand.prototype.resolveMiddleware = function (clientStack, configuration, options) {
        this.middlewareStack.use(getSerdePlugin(configuration, this.serialize, this.deserialize));
        var stack = clientStack.concat(this.middlewareStack);
        var logger = configuration.logger;
        var clientName = "CognitoIdentityClient";
        var commandName = "GetCredentialsForIdentityCommand";
        var handlerExecutionContext = {
            logger: logger,
            clientName: clientName,
            commandName: commandName,
            inputFilterSensitiveLog: GetCredentialsForIdentityInput.filterSensitiveLog,
            outputFilterSensitiveLog: GetCredentialsForIdentityResponse.filterSensitiveLog,
        };
        var requestHandler = configuration.requestHandler;
        return stack.resolve(function (request) {
            return requestHandler.handle(request.request, options || {});
        }, handlerExecutionContext);
    };
    GetCredentialsForIdentityCommand.prototype.serialize = function (input, context) {
        return serializeAws_json1_1GetCredentialsForIdentityCommand(input, context);
    };
    GetCredentialsForIdentityCommand.prototype.deserialize = function (output, context) {
        return deserializeAws_json1_1GetCredentialsForIdentityCommand(output, context);
    };
    return GetCredentialsForIdentityCommand;
}(Command));

/**
 * <p>Generates (or retrieves) a Cognito ID. Supplying multiple logins will create an
 *          implicit linked account.</p>
 *          <p>This is a public API. You do not need any credentials to call this API.</p>
 */
var GetIdCommand = /** @class */ (function (_super) {
    __extends$1(GetIdCommand, _super);
    // Start section: command_properties
    // End section: command_properties
    function GetIdCommand(input) {
        var _this = 
        // Start section: command_constructor
        _super.call(this) || this;
        _this.input = input;
        return _this;
        // End section: command_constructor
    }
    /**
     * @internal
     */
    GetIdCommand.prototype.resolveMiddleware = function (clientStack, configuration, options) {
        this.middlewareStack.use(getSerdePlugin(configuration, this.serialize, this.deserialize));
        var stack = clientStack.concat(this.middlewareStack);
        var logger = configuration.logger;
        var clientName = "CognitoIdentityClient";
        var commandName = "GetIdCommand";
        var handlerExecutionContext = {
            logger: logger,
            clientName: clientName,
            commandName: commandName,
            inputFilterSensitiveLog: GetIdInput.filterSensitiveLog,
            outputFilterSensitiveLog: GetIdResponse.filterSensitiveLog,
        };
        var requestHandler = configuration.requestHandler;
        return stack.resolve(function (request) {
            return requestHandler.handle(request.request, options || {});
        }, handlerExecutionContext);
    };
    GetIdCommand.prototype.serialize = function (input, context) {
        return serializeAws_json1_1GetIdCommand(input, context);
    };
    GetIdCommand.prototype.deserialize = function (output, context) {
        return deserializeAws_json1_1GetIdCommand(output, context);
    };
    return GetIdCommand;
}(Command));

/**
 * An error representing a failure of an individual credential provider.
 *
 * This error class has special meaning to the {@link chain} method. If a
 * provider in the chain is rejected with an error, the chain will only proceed
 * to the next provider if the value of the `tryNextLink` property on the error
 * is truthy. This allows individual providers to halt the chain and also
 * ensures the chain will stop if an entirely unexpected error is encountered.
 */
var ProviderError = /** @class */ (function (_super) {
    __extends$3(ProviderError, _super);
    function ProviderError(message, tryNextLink) {
        if (tryNextLink === void 0) { tryNextLink = true; }
        var _this = _super.call(this, message) || this;
        _this.tryNextLink = tryNextLink;
        return _this;
    }
    return ProviderError;
}(Error));

/**
 * @internal
 */
function resolveLogins(logins) {
    return Promise.all(Object.keys(logins).reduce(function (arr, name) {
        var tokenOrProvider = logins[name];
        if (typeof tokenOrProvider === "string") {
            arr.push([name, tokenOrProvider]);
        }
        else {
            arr.push(tokenOrProvider().then(function (token) { return [name, token]; }));
        }
        return arr;
    }, [])).then(function (resolvedPairs) {
        return resolvedPairs.reduce(function (logins, _a) {
            var _b = __read$4(_a, 2), key = _b[0], value = _b[1];
            logins[key] = value;
            return logins;
        }, {});
    });
}

/**
 * Retrieves temporary AWS credentials using Amazon Cognito's
 * `GetCredentialsForIdentity` operation.
 *
 * Results from this function call are not cached internally.
 */
function fromCognitoIdentity(parameters) {
    var _this = this;
    return function () { return __awaiter$7(_this, void 0, void 0, function () {
        var _a, _b, _c, AccessKeyId, Expiration, _d, SecretKey, SessionToken, _e, _f, _g, _h;
        var _j;
        return __generator$7(this, function (_k) {
            switch (_k.label) {
                case 0:
                    _f = (_e = parameters.client).send;
                    _g = GetCredentialsForIdentityCommand.bind;
                    _j = {
                        CustomRoleArn: parameters.customRoleArn,
                        IdentityId: parameters.identityId
                    };
                    if (!parameters.logins) return [3 /*break*/, 2];
                    return [4 /*yield*/, resolveLogins(parameters.logins)];
                case 1:
                    _h = _k.sent();
                    return [3 /*break*/, 3];
                case 2:
                    _h = undefined;
                    _k.label = 3;
                case 3: return [4 /*yield*/, _f.apply(_e, [new (_g.apply(GetCredentialsForIdentityCommand, [void 0, (_j.Logins = _h,
                                _j)]))()])];
                case 4:
                    _a = (_k.sent()).Credentials, _b = _a === void 0 ? throwOnMissingCredentials() : _a, _c = _b.AccessKeyId, AccessKeyId = _c === void 0 ? throwOnMissingAccessKeyId() : _c, Expiration = _b.Expiration, _d = _b.SecretKey, SecretKey = _d === void 0 ? throwOnMissingSecretKey() : _d, SessionToken = _b.SessionToken;
                    return [2 /*return*/, {
                            identityId: parameters.identityId,
                            accessKeyId: AccessKeyId,
                            secretAccessKey: SecretKey,
                            sessionToken: SessionToken,
                            expiration: Expiration,
                        }];
            }
        });
    }); };
}
function throwOnMissingAccessKeyId() {
    throw new ProviderError("Response from Amazon Cognito contained no access key ID");
}
function throwOnMissingCredentials() {
    throw new ProviderError("Response from Amazon Cognito contained no credentials");
}
function throwOnMissingSecretKey() {
    throw new ProviderError("Response from Amazon Cognito contained no secret key");
}

var STORE_NAME = "IdentityIds";
var IndexedDbStorage = /** @class */ (function () {
    function IndexedDbStorage(dbName) {
        if (dbName === void 0) { dbName = "aws:cognito-identity-ids"; }
        this.dbName = dbName;
    }
    IndexedDbStorage.prototype.getItem = function (key) {
        return this.withObjectStore("readonly", function (store) {
            var req = store.get(key);
            return new Promise(function (resolve) {
                req.onerror = function () { return resolve(null); };
                req.onsuccess = function () { return resolve(req.result ? req.result.value : null); };
            });
        }).catch(function () { return null; });
    };
    IndexedDbStorage.prototype.removeItem = function (key) {
        return this.withObjectStore("readwrite", function (store) {
            var req = store.delete(key);
            return new Promise(function (resolve, reject) {
                req.onerror = function () { return reject(req.error); };
                req.onsuccess = function () { return resolve(); };
            });
        });
    };
    IndexedDbStorage.prototype.setItem = function (id, value) {
        return this.withObjectStore("readwrite", function (store) {
            var req = store.put({ id: id, value: value });
            return new Promise(function (resolve, reject) {
                req.onerror = function () { return reject(req.error); };
                req.onsuccess = function () { return resolve(); };
            });
        });
    };
    IndexedDbStorage.prototype.getDb = function () {
        var openDbRequest = self.indexedDB.open(this.dbName, 1);
        return new Promise(function (resolve, reject) {
            openDbRequest.onsuccess = function () {
                resolve(openDbRequest.result);
            };
            openDbRequest.onerror = function () {
                reject(openDbRequest.error);
            };
            openDbRequest.onblocked = function () {
                reject(new Error("Unable to access DB"));
            };
            openDbRequest.onupgradeneeded = function () {
                var db = openDbRequest.result;
                db.onerror = function () {
                    reject(new Error("Failed to create object store"));
                };
                db.createObjectStore(STORE_NAME, { keyPath: "id" });
            };
        });
    };
    IndexedDbStorage.prototype.withObjectStore = function (mode, action) {
        return this.getDb().then(function (db) {
            var tx = db.transaction(STORE_NAME, mode);
            tx.oncomplete = function () { return db.close(); };
            return new Promise(function (resolve, reject) {
                tx.onerror = function () { return reject(tx.error); };
                resolve(action(tx.objectStore(STORE_NAME)));
            }).catch(function (err) {
                db.close();
                throw err;
            });
        });
    };
    return IndexedDbStorage;
}());

var InMemoryStorage = /** @class */ (function () {
    function InMemoryStorage(store) {
        if (store === void 0) { store = {}; }
        this.store = store;
    }
    InMemoryStorage.prototype.getItem = function (key) {
        if (key in this.store) {
            return this.store[key];
        }
        return null;
    };
    InMemoryStorage.prototype.removeItem = function (key) {
        delete this.store[key];
    };
    InMemoryStorage.prototype.setItem = function (key, value) {
        this.store[key] = value;
    };
    return InMemoryStorage;
}());

var inMemoryStorage = new InMemoryStorage();
function localStorage() {
    if (typeof self === "object" && self.indexedDB) {
        return new IndexedDbStorage();
    }
    if (typeof window === "object" && window.localStorage) {
        return window.localStorage;
    }
    return inMemoryStorage;
}

/**
 * Retrieves or generates a unique identifier using Amazon Cognito's `GetId`
 * operation, then generates temporary AWS credentials using Amazon Cognito's
 * `GetCredentialsForIdentity` operation.
 *
 * Results from `GetId` are cached internally, but results from
 * `GetCredentialsForIdentity` are not.
 */
function fromCognitoIdentityPool(_a) {
    var _this = this;
    var accountId = _a.accountId, _b = _a.cache, cache = _b === void 0 ? localStorage() : _b, client = _a.client, customRoleArn = _a.customRoleArn, identityPoolId = _a.identityPoolId, logins = _a.logins, _c = _a.userIdentifier, userIdentifier = _c === void 0 ? !logins || Object.keys(logins).length === 0 ? "ANONYMOUS" : undefined : _c;
    var cacheKey = userIdentifier ? "aws:cognito-identity-credentials:" + identityPoolId + ":" + userIdentifier : undefined;
    var provider = function () { return __awaiter$7(_this, void 0, void 0, function () {
        var identityId, _a, _b, IdentityId, _c, _d, _e, _f;
        var _g;
        return __generator$7(this, function (_h) {
            switch (_h.label) {
                case 0:
                    _a = cacheKey;
                    if (!_a) return [3 /*break*/, 2];
                    return [4 /*yield*/, cache.getItem(cacheKey)];
                case 1:
                    _a = (_h.sent());
                    _h.label = 2;
                case 2:
                    identityId = _a;
                    if (!!identityId) return [3 /*break*/, 7];
                    _d = (_c = client).send;
                    _e = GetIdCommand.bind;
                    _g = {
                        AccountId: accountId,
                        IdentityPoolId: identityPoolId
                    };
                    if (!logins) return [3 /*break*/, 4];
                    return [4 /*yield*/, resolveLogins(logins)];
                case 3:
                    _f = _h.sent();
                    return [3 /*break*/, 5];
                case 4:
                    _f = undefined;
                    _h.label = 5;
                case 5: return [4 /*yield*/, _d.apply(_c, [new (_e.apply(GetIdCommand, [void 0, (_g.Logins = _f,
                                _g)]))()])];
                case 6:
                    _b = (_h.sent()).IdentityId, IdentityId = _b === void 0 ? throwOnMissingId() : _b;
                    identityId = IdentityId;
                    if (cacheKey) {
                        Promise.resolve(cache.setItem(cacheKey, identityId)).catch(function () { });
                    }
                    _h.label = 7;
                case 7:
                    provider = fromCognitoIdentity({
                        client: client,
                        customRoleArn: customRoleArn,
                        logins: logins,
                        identityId: identityId,
                    });
                    return [2 /*return*/, provider()];
            }
        });
    }); };
    return function () {
        return provider().catch(function (err) { return __awaiter$7(_this, void 0, void 0, function () {
            return __generator$7(this, function (_a) {
                if (cacheKey) {
                    Promise.resolve(cache.removeItem(cacheKey)).catch(function () { });
                }
                throw err;
            });
        }); });
    };
}
function throwOnMissingId() {
    throw new ProviderError("Response from Amazon Cognito contained no identity ID");
}

var __assign$3 = (undefined && undefined.__assign) || function () {
    __assign$3 = Object.assign || function(t) {
        for (var s, i = 1, n = arguments.length; i < n; i++) {
            s = arguments[i];
            for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
                t[p] = s[p];
        }
        return t;
    };
    return __assign$3.apply(this, arguments);
};
var __awaiter$2 = (undefined && undefined.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator$2 = (undefined && undefined.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
var logger$4 = new ConsoleLogger('Credentials');
var CREDENTIALS_TTL = 50 * 60 * 1000; // 50 min, can be modified on config if required in the future
var COGNITO_IDENTITY_KEY_PREFIX = 'CognitoIdentityId-';
var CredentialsClass = /** @class */ (function () {
    function CredentialsClass(config) {
        this._gettingCredPromise = null;
        this._refreshHandlers = {};
        // Allow `Auth` to be injected for SSR, but Auth isn't a required dependency for Credentials
        this.Auth = undefined;
        this.configure(config);
        this._refreshHandlers['google'] = GoogleOAuth.refreshGoogleToken;
        this._refreshHandlers['facebook'] = FacebookOAuth.refreshFacebookToken;
    }
    CredentialsClass.prototype.getModuleName = function () {
        return 'Credentials';
    };
    CredentialsClass.prototype.getCredSource = function () {
        return this._credentials_source;
    };
    CredentialsClass.prototype.configure = function (config) {
        if (!config)
            return this._config || {};
        this._config = Object.assign({}, this._config, config);
        var refreshHandlers = this._config.refreshHandlers;
        // If the developer has provided an object of refresh handlers,
        // then we can merge the provided handlers with the current handlers.
        if (refreshHandlers) {
            this._refreshHandlers = __assign$3(__assign$3({}, this._refreshHandlers), refreshHandlers);
        }
        this._storage = this._config.storage;
        if (!this._storage) {
            this._storage = new StorageHelper$1().getStorage();
        }
        this._storageSync = Promise.resolve();
        if (typeof this._storage['sync'] === 'function') {
            this._storageSync = this._storage['sync']();
        }
        return this._config;
    };
    CredentialsClass.prototype.get = function () {
        logger$4.debug('getting credentials');
        return this._pickupCredentials();
    };
    // currently we only store the guest identity in local storage
    CredentialsClass.prototype._getCognitoIdentityIdStorageKey = function (identityPoolId) {
        return "" + COGNITO_IDENTITY_KEY_PREFIX + identityPoolId;
    };
    CredentialsClass.prototype._pickupCredentials = function () {
        logger$4.debug('picking up credentials');
        if (!this._gettingCredPromise || !this._gettingCredPromise.isPending()) {
            logger$4.debug('getting new cred promise');
            this._gettingCredPromise = makeQuerablePromise(this._keepAlive());
        }
        else {
            logger$4.debug('getting old cred promise');
        }
        return this._gettingCredPromise;
    };
    CredentialsClass.prototype._keepAlive = function () {
        return __awaiter$2(this, void 0, void 0, function () {
            var cred, _a, Auth, user_1, session, refreshToken_1, refreshRequest, err_1;
            return __generator$2(this, function (_b) {
                switch (_b.label) {
                    case 0:
                        logger$4.debug('checking if credentials exists and not expired');
                        cred = this._credentials;
                        if (cred && !this._isExpired(cred) && !this._isPastTTL()) {
                            logger$4.debug('credentials not changed and not expired, directly return');
                            return [2 /*return*/, Promise.resolve(cred)];
                        }
                        logger$4.debug('need to get a new credential or refresh the existing one');
                        _a = this.Auth, Auth = _a === void 0 ? Amplify.Auth : _a;
                        if (!Auth || typeof Auth.currentUserCredentials !== 'function') {
                            return [2 /*return*/, Promise.reject('No Auth module registered in Amplify')];
                        }
                        if (!(!this._isExpired(cred) && this._isPastTTL())) return [3 /*break*/, 6];
                        logger$4.debug('ttl has passed but token is not yet expired');
                        _b.label = 1;
                    case 1:
                        _b.trys.push([1, 5, , 6]);
                        return [4 /*yield*/, Auth.currentUserPoolUser()];
                    case 2:
                        user_1 = _b.sent();
                        return [4 /*yield*/, Auth.currentSession()];
                    case 3:
                        session = _b.sent();
                        refreshToken_1 = session.refreshToken;
                        refreshRequest = new Promise(function (res, rej) {
                            user_1.refreshSession(refreshToken_1, function (err, data) {
                                return err ? rej(err) : res(data);
                            });
                        });
                        return [4 /*yield*/, refreshRequest];
                    case 4:
                        _b.sent(); // note that rejections will be caught and handled in the catch block.
                        return [3 /*break*/, 6];
                    case 5:
                        err_1 = _b.sent();
                        // should not throw because user might just be on guest access or is authenticated through federation
                        logger$4.debug('Error attempting to refreshing the session', err_1);
                        return [3 /*break*/, 6];
                    case 6: return [2 /*return*/, Auth.currentUserCredentials()];
                }
            });
        });
    };
    CredentialsClass.prototype.refreshFederatedToken = function (federatedInfo) {
        logger$4.debug('Getting federated credentials');
        var provider = federatedInfo.provider, user = federatedInfo.user, token = federatedInfo.token, identity_id = federatedInfo.identity_id;
        var expires_at = federatedInfo.expires_at;
        // Make sure expires_at is in millis
        expires_at =
            new Date(expires_at).getFullYear() === 1970
                ? expires_at * 1000
                : expires_at;
        var that = this;
        logger$4.debug('checking if federated jwt token expired');
        if (expires_at > new Date().getTime()) {
            // if not expired
            logger$4.debug('token not expired');
            return this._setCredentialsFromFederation({
                provider: provider,
                token: token,
                user: user,
                identity_id: identity_id,
                expires_at: expires_at,
            });
        }
        else {
            // if refresh handler exists
            if (that._refreshHandlers[provider] &&
                typeof that._refreshHandlers[provider] === 'function') {
                logger$4.debug('getting refreshed jwt token from federation provider');
                return this._providerRefreshWithRetry({
                    refreshHandler: that._refreshHandlers[provider],
                    provider: provider,
                    user: user,
                });
            }
            else {
                logger$4.debug('no refresh handler for provider:', provider);
                this.clear();
                return Promise.reject('no refresh handler for provider');
            }
        }
    };
    CredentialsClass.prototype._providerRefreshWithRetry = function (_a) {
        var _this = this;
        var refreshHandler = _a.refreshHandler, provider = _a.provider, user = _a.user;
        var MAX_DELAY_MS = 10 * 1000;
        // refreshHandler will retry network errors, otherwise it will
        // return NonRetryableError to break out of jitteredExponentialRetry
        return jitteredExponentialRetry$1(refreshHandler, [], MAX_DELAY_MS)
            .then(function (data) {
            logger$4.debug('refresh federated token sucessfully', data);
            return _this._setCredentialsFromFederation({
                provider: provider,
                token: data.token,
                user: user,
                identity_id: data.identity_id,
                expires_at: data.expires_at,
            });
        })
            .catch(function (e) {
            var isNetworkError = typeof e === 'string' &&
                e.toLowerCase().lastIndexOf('network error', e.length) === 0;
            if (!isNetworkError) {
                _this.clear();
            }
            logger$4.debug('refresh federated token failed', e);
            return Promise.reject('refreshing federation token failed: ' + e);
        });
    };
    CredentialsClass.prototype._isExpired = function (credentials) {
        if (!credentials) {
            logger$4.debug('no credentials for expiration check');
            return true;
        }
        logger$4.debug('are these credentials expired?', credentials);
        var ts = Date.now();
        /* returns date object.
            https://github.com/aws/aws-sdk-js-v3/blob/v1.0.0-beta.1/packages/types/src/credentials.ts#L26
        */
        var expiration = credentials.expiration;
        return expiration.getTime() <= ts;
    };
    CredentialsClass.prototype._isPastTTL = function () {
        return this._nextCredentialsRefresh <= Date.now();
    };
    CredentialsClass.prototype._setCredentialsForGuest = function () {
        return __awaiter$2(this, void 0, void 0, function () {
            var _a, identityPoolId, region, mandatorySignIn, identityId, _b, cognitoClient, credentials, cognitoIdentityParams, credentialsProvider;
            var _this = this;
            return __generator$2(this, function (_c) {
                switch (_c.label) {
                    case 0:
                        logger$4.debug('setting credentials for guest');
                        _a = this._config, identityPoolId = _a.identityPoolId, region = _a.region, mandatorySignIn = _a.mandatorySignIn;
                        if (mandatorySignIn) {
                            return [2 /*return*/, Promise.reject('cannot get guest credentials when mandatory signin enabled')];
                        }
                        if (!identityPoolId) {
                            logger$4.debug('No Cognito Identity pool provided for unauthenticated access');
                            return [2 /*return*/, Promise.reject('No Cognito Identity pool provided for unauthenticated access')];
                        }
                        if (!region) {
                            logger$4.debug('region is not configured for getting the credentials');
                            return [2 /*return*/, Promise.reject('region is not configured for getting the credentials')];
                        }
                        _b = this;
                        return [4 /*yield*/, this._getGuestIdentityId()];
                    case 1:
                        identityId = _b._identityId = _c.sent();
                        cognitoClient = new CognitoIdentityClient({
                            region: region,
                            customUserAgent: getAmplifyUserAgent(),
                        });
                        credentials = undefined;
                        if (identityId) {
                            cognitoIdentityParams = {
                                identityId: identityId,
                                client: cognitoClient,
                            };
                            credentials = fromCognitoIdentity(cognitoIdentityParams)();
                        }
                        else {
                            credentialsProvider = function () { return __awaiter$2(_this, void 0, void 0, function () {
                                var IdentityId, cognitoIdentityParams, credentialsFromCognitoIdentity;
                                return __generator$2(this, function (_a) {
                                    switch (_a.label) {
                                        case 0: return [4 /*yield*/, cognitoClient.send(new GetIdCommand({
                                                IdentityPoolId: identityPoolId,
                                            }))];
                                        case 1:
                                            IdentityId = (_a.sent()).IdentityId;
                                            this._identityId = IdentityId;
                                            cognitoIdentityParams = {
                                                client: cognitoClient,
                                                identityId: IdentityId,
                                            };
                                            credentialsFromCognitoIdentity = fromCognitoIdentity(cognitoIdentityParams);
                                            return [2 /*return*/, credentialsFromCognitoIdentity()];
                                    }
                                });
                            }); };
                            credentials = credentialsProvider().catch(function (err) { return __awaiter$2(_this, void 0, void 0, function () {
                                return __generator$2(this, function (_a) {
                                    throw err;
                                });
                            }); });
                        }
                        return [2 /*return*/, this._loadCredentials(credentials, 'guest', false, null)
                                .then(function (res) {
                                return res;
                            })
                                .catch(function (e) { return __awaiter$2(_this, void 0, void 0, function () {
                                var credentialsProvider;
                                var _this = this;
                                return __generator$2(this, function (_a) {
                                    switch (_a.label) {
                                        case 0:
                                            if (!(e.name === 'ResourceNotFoundException' &&
                                                e.message === "Identity '" + identityId + "' not found.")) return [3 /*break*/, 2];
                                            logger$4.debug('Failed to load guest credentials');
                                            return [4 /*yield*/, this._removeGuestIdentityId()];
                                        case 1:
                                            _a.sent();
                                            credentialsProvider = function () { return __awaiter$2(_this, void 0, void 0, function () {
                                                var IdentityId, cognitoIdentityParams, credentialsFromCognitoIdentity;
                                                return __generator$2(this, function (_a) {
                                                    switch (_a.label) {
                                                        case 0: return [4 /*yield*/, cognitoClient.send(new GetIdCommand({
                                                                IdentityPoolId: identityPoolId,
                                                            }))];
                                                        case 1:
                                                            IdentityId = (_a.sent()).IdentityId;
                                                            this._identityId = IdentityId;
                                                            cognitoIdentityParams = {
                                                                client: cognitoClient,
                                                                identityId: IdentityId,
                                                            };
                                                            credentialsFromCognitoIdentity = fromCognitoIdentity(cognitoIdentityParams);
                                                            return [2 /*return*/, credentialsFromCognitoIdentity()];
                                                    }
                                                });
                                            }); };
                                            credentials = credentialsProvider().catch(function (err) { return __awaiter$2(_this, void 0, void 0, function () {
                                                return __generator$2(this, function (_a) {
                                                    throw err;
                                                });
                                            }); });
                                            return [2 /*return*/, this._loadCredentials(credentials, 'guest', false, null)];
                                        case 2: return [2 /*return*/, e];
                                    }
                                });
                            }); })];
                }
            });
        });
    };
    CredentialsClass.prototype._setCredentialsFromFederation = function (params) {
        var provider = params.provider, token = params.token, identity_id = params.identity_id;
        var domains = {
            google: 'accounts.google.com',
            facebook: 'graph.facebook.com',
            amazon: 'www.amazon.com',
            developer: 'cognito-identity.amazonaws.com',
        };
        // Use custom provider url instead of the predefined ones
        var domain = domains[provider] || provider;
        if (!domain) {
            return Promise.reject('You must specify a federated provider');
        }
        var logins = {};
        logins[domain] = token;
        var _a = this._config, identityPoolId = _a.identityPoolId, region = _a.region;
        if (!identityPoolId) {
            logger$4.debug('No Cognito Federated Identity pool provided');
            return Promise.reject('No Cognito Federated Identity pool provided');
        }
        if (!region) {
            logger$4.debug('region is not configured for getting the credentials');
            return Promise.reject('region is not configured for getting the credentials');
        }
        var cognitoClient = new CognitoIdentityClient({
            region: region,
            customUserAgent: getAmplifyUserAgent(),
        });
        var credentials = undefined;
        if (identity_id) {
            var cognitoIdentityParams = {
                identityId: identity_id,
                logins: logins,
                client: cognitoClient,
            };
            credentials = fromCognitoIdentity(cognitoIdentityParams)();
        }
        else {
            var cognitoIdentityParams = {
                logins: logins,
                identityPoolId: identityPoolId,
                client: cognitoClient,
            };
            credentials = fromCognitoIdentityPool(cognitoIdentityParams)();
        }
        return this._loadCredentials(credentials, 'federated', true, params);
    };
    CredentialsClass.prototype._setCredentialsFromSession = function (session) {
        var _this = this;
        logger$4.debug('set credentials from session');
        var idToken = session.getIdToken().getJwtToken();
        var _a = this._config, region = _a.region, userPoolId = _a.userPoolId, identityPoolId = _a.identityPoolId;
        if (!identityPoolId) {
            logger$4.debug('No Cognito Federated Identity pool provided');
            return Promise.reject('No Cognito Federated Identity pool provided');
        }
        if (!region) {
            logger$4.debug('region is not configured for getting the credentials');
            return Promise.reject('region is not configured for getting the credentials');
        }
        var key = 'cognito-idp.' + region + '.amazonaws.com/' + userPoolId;
        var logins = {};
        logins[key] = idToken;
        var cognitoClient = new CognitoIdentityClient({
            region: region,
            customUserAgent: getAmplifyUserAgent(),
        });
        /*
            Retreiving identityId with GetIdCommand to mimic the behavior in the following code in aws-sdk-v3:
            https://git.io/JeDxU

            Note: Retreive identityId from CredentialsProvider once aws-sdk-js v3 supports this.
        */
        var credentialsProvider = function () { return __awaiter$2(_this, void 0, void 0, function () {
            var guestIdentityId, generatedOrRetrievedIdentityId, IdentityId, _a, _b, AccessKeyId, Expiration, SecretKey, SessionToken, primaryIdentityId;
            return __generator$2(this, function (_c) {
                switch (_c.label) {
                    case 0: return [4 /*yield*/, this._getGuestIdentityId()];
                    case 1:
                        guestIdentityId = _c.sent();
                        if (!!guestIdentityId) return [3 /*break*/, 3];
                        return [4 /*yield*/, cognitoClient.send(new GetIdCommand({
                                IdentityPoolId: identityPoolId,
                                Logins: logins,
                            }))];
                    case 2:
                        IdentityId = (_c.sent()).IdentityId;
                        generatedOrRetrievedIdentityId = IdentityId;
                        _c.label = 3;
                    case 3: return [4 /*yield*/, cognitoClient.send(new GetCredentialsForIdentityCommand({
                            IdentityId: guestIdentityId || generatedOrRetrievedIdentityId,
                            Logins: logins,
                        }))];
                    case 4:
                        _a = _c.sent(), _b = _a.Credentials, AccessKeyId = _b.AccessKeyId, Expiration = _b.Expiration, SecretKey = _b.SecretKey, SessionToken = _b.SessionToken, primaryIdentityId = _a.IdentityId;
                        this._identityId = primaryIdentityId;
                        if (!guestIdentityId) return [3 /*break*/, 6];
                        // if guestIdentity is found and used by GetCredentialsForIdentity
                        // it will be linked to the logins provided, and disqualified as an unauth identity
                        logger$4.debug("The guest identity " + guestIdentityId + " has been successfully linked to the logins");
                        if (guestIdentityId === primaryIdentityId) {
                            logger$4.debug("The guest identity " + guestIdentityId + " has become the primary identity");
                        }
                        // remove it from local storage to avoid being used as a guest Identity by _setCredentialsForGuest
                        return [4 /*yield*/, this._removeGuestIdentityId()];
                    case 5:
                        // remove it from local storage to avoid being used as a guest Identity by _setCredentialsForGuest
                        _c.sent();
                        _c.label = 6;
                    case 6: 
                    // https://github.com/aws/aws-sdk-js-v3/blob/main/packages/credential-provider-cognito-identity/src/fromCognitoIdentity.ts#L40
                    return [2 /*return*/, {
                            accessKeyId: AccessKeyId,
                            secretAccessKey: SecretKey,
                            sessionToken: SessionToken,
                            expiration: Expiration,
                            identityId: primaryIdentityId,
                        }];
                }
            });
        }); };
        var credentials = credentialsProvider().catch(function (err) { return __awaiter$2(_this, void 0, void 0, function () {
            return __generator$2(this, function (_a) {
                throw err;
            });
        }); });
        return this._loadCredentials(credentials, 'userPool', true, null);
    };
    CredentialsClass.prototype._loadCredentials = function (credentials, source, authenticated, info) {
        var _this = this;
        var that = this;
        return new Promise(function (res, rej) {
            credentials
                .then(function (credentials) { return __awaiter$2(_this, void 0, void 0, function () {
                var user, provider, token, expires_at, identity_id;
                return __generator$2(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            logger$4.debug('Load credentials successfully', credentials);
                            if (this._identityId && !credentials.identityId) {
                                credentials['identityId'] = this._identityId;
                            }
                            that._credentials = credentials;
                            that._credentials.authenticated = authenticated;
                            that._credentials_source = source;
                            that._nextCredentialsRefresh = new Date().getTime() + CREDENTIALS_TTL;
                            if (source === 'federated') {
                                user = Object.assign({ id: this._credentials.identityId }, info.user);
                                provider = info.provider, token = info.token, expires_at = info.expires_at, identity_id = info.identity_id;
                                try {
                                    this._storage.setItem('aws-amplify-federatedInfo', JSON.stringify({
                                        provider: provider,
                                        token: token,
                                        user: user,
                                        expires_at: expires_at,
                                        identity_id: identity_id,
                                    }));
                                }
                                catch (e) {
                                    logger$4.debug('Failed to put federated info into auth storage', e);
                                }
                            }
                            if (!(source === 'guest')) return [3 /*break*/, 2];
                            return [4 /*yield*/, this._setGuestIdentityId(credentials.identityId)];
                        case 1:
                            _a.sent();
                            _a.label = 2;
                        case 2:
                            res(that._credentials);
                            return [2 /*return*/];
                    }
                });
            }); })
                .catch(function (err) {
                if (err) {
                    logger$4.debug('Failed to load credentials', credentials);
                    logger$4.debug('Error loading credentials', err);
                    rej(err);
                    return;
                }
            });
        });
    };
    CredentialsClass.prototype.set = function (params, source) {
        if (source === 'session') {
            return this._setCredentialsFromSession(params);
        }
        else if (source === 'federation') {
            return this._setCredentialsFromFederation(params);
        }
        else if (source === 'guest') {
            return this._setCredentialsForGuest();
        }
        else {
            logger$4.debug('no source specified for setting credentials');
            return Promise.reject('invalid source');
        }
    };
    CredentialsClass.prototype.clear = function () {
        return __awaiter$2(this, void 0, void 0, function () {
            return __generator$2(this, function (_a) {
                this._credentials = null;
                this._credentials_source = null;
                logger$4.debug('removing aws-amplify-federatedInfo from storage');
                this._storage.removeItem('aws-amplify-federatedInfo');
                return [2 /*return*/];
            });
        });
    };
    /* operations on local stored guest identity */
    CredentialsClass.prototype._getGuestIdentityId = function () {
        return __awaiter$2(this, void 0, void 0, function () {
            var identityPoolId, e_1;
            return __generator$2(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        identityPoolId = this._config.identityPoolId;
                        _a.label = 1;
                    case 1:
                        _a.trys.push([1, 3, , 4]);
                        return [4 /*yield*/, this._storageSync];
                    case 2:
                        _a.sent();
                        return [2 /*return*/, this._storage.getItem(this._getCognitoIdentityIdStorageKey(identityPoolId))];
                    case 3:
                        e_1 = _a.sent();
                        logger$4.debug('Failed to get the cached guest identityId', e_1);
                        return [3 /*break*/, 4];
                    case 4: return [2 /*return*/];
                }
            });
        });
    };
    CredentialsClass.prototype._setGuestIdentityId = function (identityId) {
        return __awaiter$2(this, void 0, void 0, function () {
            var identityPoolId, e_2;
            return __generator$2(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        identityPoolId = this._config.identityPoolId;
                        _a.label = 1;
                    case 1:
                        _a.trys.push([1, 3, , 4]);
                        return [4 /*yield*/, this._storageSync];
                    case 2:
                        _a.sent();
                        this._storage.setItem(this._getCognitoIdentityIdStorageKey(identityPoolId), identityId);
                        return [3 /*break*/, 4];
                    case 3:
                        e_2 = _a.sent();
                        logger$4.debug('Failed to cache guest identityId', e_2);
                        return [3 /*break*/, 4];
                    case 4: return [2 /*return*/];
                }
            });
        });
    };
    CredentialsClass.prototype._removeGuestIdentityId = function () {
        return __awaiter$2(this, void 0, void 0, function () {
            var identityPoolId;
            return __generator$2(this, function (_a) {
                identityPoolId = this._config.identityPoolId;
                logger$4.debug("removing " + this._getCognitoIdentityIdStorageKey(identityPoolId) + " from storage");
                this._storage.removeItem(this._getCognitoIdentityIdStorageKey(identityPoolId));
                return [2 /*return*/];
            });
        });
    };
    /**
     * Compact version of credentials
     * @param {Object} credentials
     * @return {Object} - Credentials
     */
    CredentialsClass.prototype.shear = function (credentials) {
        return {
            accessKeyId: credentials.accessKeyId,
            sessionToken: credentials.sessionToken,
            secretAccessKey: credentials.secretAccessKey,
            identityId: credentials.identityId,
            authenticated: credentials.authenticated,
        };
    };
    return CredentialsClass;
}());
var Credentials = new CredentialsClass(null);
Amplify.register(Credentials);

/*!
 * cookie
 * Copyright(c) 2012-2014 Roman Shtylman
 * Copyright(c) 2015 Douglas Christopher Wilson
 * MIT Licensed
 */

/**
 * Module exports.
 * @public
 */

var parse_1 = parse;
var serialize_1 = serialize;

/**
 * Module variables.
 * @private
 */

var decode = decodeURIComponent;
var encode = encodeURIComponent;

/**
 * RegExp to match field-content in RFC 7230 sec 3.2
 *
 * field-content = field-vchar [ 1*( SP / HTAB ) field-vchar ]
 * field-vchar   = VCHAR / obs-text
 * obs-text      = %x80-FF
 */

var fieldContentRegExp = /^[\u0009\u0020-\u007e\u0080-\u00ff]+$/;

/**
 * Parse a cookie header.
 *
 * Parse the given cookie header string into an object
 * The object has the various cookies as keys(names) => values
 *
 * @param {string} str
 * @param {object} [options]
 * @return {object}
 * @public
 */

function parse(str, options) {
  if (typeof str !== 'string') {
    throw new TypeError('argument str must be a string');
  }

  var obj = {};
  var opt = options || {};
  var pairs = str.split(';');
  var dec = opt.decode || decode;

  for (var i = 0; i < pairs.length; i++) {
    var pair = pairs[i];
    var index = pair.indexOf('=');

    // skip things that don't look like key=value
    if (index < 0) {
      continue;
    }

    var key = pair.substring(0, index).trim();

    // only assign once
    if (undefined == obj[key]) {
      var val = pair.substring(index + 1, pair.length).trim();

      // quoted values
      if (val[0] === '"') {
        val = val.slice(1, -1);
      }

      obj[key] = tryDecode(val, dec);
    }
  }

  return obj;
}

/**
 * Serialize data into a cookie header.
 *
 * Serialize the a name value pair into a cookie string suitable for
 * http headers. An optional options object specified cookie parameters.
 *
 * serialize('foo', 'bar', { httpOnly: true })
 *   => "foo=bar; httpOnly"
 *
 * @param {string} name
 * @param {string} val
 * @param {object} [options]
 * @return {string}
 * @public
 */

function serialize(name, val, options) {
  var opt = options || {};
  var enc = opt.encode || encode;

  if (typeof enc !== 'function') {
    throw new TypeError('option encode is invalid');
  }

  if (!fieldContentRegExp.test(name)) {
    throw new TypeError('argument name is invalid');
  }

  var value = enc(val);

  if (value && !fieldContentRegExp.test(value)) {
    throw new TypeError('argument val is invalid');
  }

  var str = name + '=' + value;

  if (null != opt.maxAge) {
    var maxAge = opt.maxAge - 0;

    if (isNaN(maxAge) || !isFinite(maxAge)) {
      throw new TypeError('option maxAge is invalid')
    }

    str += '; Max-Age=' + Math.floor(maxAge);
  }

  if (opt.domain) {
    if (!fieldContentRegExp.test(opt.domain)) {
      throw new TypeError('option domain is invalid');
    }

    str += '; Domain=' + opt.domain;
  }

  if (opt.path) {
    if (!fieldContentRegExp.test(opt.path)) {
      throw new TypeError('option path is invalid');
    }

    str += '; Path=' + opt.path;
  }

  if (opt.expires) {
    if (typeof opt.expires.toUTCString !== 'function') {
      throw new TypeError('option expires is invalid');
    }

    str += '; Expires=' + opt.expires.toUTCString();
  }

  if (opt.httpOnly) {
    str += '; HttpOnly';
  }

  if (opt.secure) {
    str += '; Secure';
  }

  if (opt.sameSite) {
    var sameSite = typeof opt.sameSite === 'string'
      ? opt.sameSite.toLowerCase() : opt.sameSite;

    switch (sameSite) {
      case true:
        str += '; SameSite=Strict';
        break;
      case 'lax':
        str += '; SameSite=Lax';
        break;
      case 'strict':
        str += '; SameSite=Strict';
        break;
      case 'none':
        str += '; SameSite=None';
        break;
      default:
        throw new TypeError('option sameSite is invalid');
    }
  }

  return str;
}

/**
 * Try decoding a string using a decoding function.
 *
 * @param {string} str
 * @param {function} decode
 * @private
 */

function tryDecode(str, decode) {
  try {
    return decode(str);
  } catch (e) {
    return str;
  }
}

function hasDocumentCookie() {
    // Can we get/set cookies on document.cookie?
    return typeof document === 'object' && typeof document.cookie === 'string';
}
function parseCookies(cookies, options) {
    if (typeof cookies === 'string') {
        return parse_1(cookies, options);
    }
    else if (typeof cookies === 'object' && cookies !== null) {
        return cookies;
    }
    else {
        return {};
    }
}
function isParsingCookie(value, doNotParse) {
    if (typeof doNotParse === 'undefined') {
        // We guess if the cookie start with { or [, it has been serialized
        doNotParse =
            !value || (value[0] !== '{' && value[0] !== '[' && value[0] !== '"');
    }
    return !doNotParse;
}
function readCookie(value, options) {
    if (options === void 0) { options = {}; }
    var cleanValue = cleanupCookieValue(value);
    if (isParsingCookie(cleanValue, options.doNotParse)) {
        try {
            return JSON.parse(cleanValue);
        }
        catch (e) {
            // At least we tried
        }
    }
    // Ignore clean value if we failed the deserialization
    // It is not relevant anymore to trim those values
    return value;
}
function cleanupCookieValue(value) {
    // express prepend j: before serializing a cookie
    if (value && value[0] === 'j' && value[1] === ':') {
        return value.substr(2);
    }
    return value;
}

var __assign$2 = (undefined && undefined.__assign) || function () {
    __assign$2 = Object.assign || function(t) {
        for (var s, i = 1, n = arguments.length; i < n; i++) {
            s = arguments[i];
            for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
                t[p] = s[p];
        }
        return t;
    };
    return __assign$2.apply(this, arguments);
};
var Cookies = /** @class */ (function () {
    function Cookies(cookies, options) {
        var _this = this;
        this.changeListeners = [];
        this.HAS_DOCUMENT_COOKIE = false;
        this.cookies = parseCookies(cookies, options);
        new Promise(function () {
            _this.HAS_DOCUMENT_COOKIE = hasDocumentCookie();
        }).catch(function () { });
    }
    Cookies.prototype._updateBrowserValues = function (parseOptions) {
        if (!this.HAS_DOCUMENT_COOKIE) {
            return;
        }
        this.cookies = parse_1(document.cookie, parseOptions);
    };
    Cookies.prototype._emitChange = function (params) {
        for (var i = 0; i < this.changeListeners.length; ++i) {
            this.changeListeners[i](params);
        }
    };
    Cookies.prototype.get = function (name, options, parseOptions) {
        if (options === void 0) { options = {}; }
        this._updateBrowserValues(parseOptions);
        return readCookie(this.cookies[name], options);
    };
    Cookies.prototype.getAll = function (options, parseOptions) {
        if (options === void 0) { options = {}; }
        this._updateBrowserValues(parseOptions);
        var result = {};
        for (var name_1 in this.cookies) {
            result[name_1] = readCookie(this.cookies[name_1], options);
        }
        return result;
    };
    Cookies.prototype.set = function (name, value, options) {
        var _a;
        if (typeof value === 'object') {
            value = JSON.stringify(value);
        }
        this.cookies = __assign$2(__assign$2({}, this.cookies), (_a = {}, _a[name] = value, _a));
        if (this.HAS_DOCUMENT_COOKIE) {
            document.cookie = serialize_1(name, value, options);
        }
        this._emitChange({ name: name, value: value, options: options });
    };
    Cookies.prototype.remove = function (name, options) {
        var finalOptions = (options = __assign$2(__assign$2({}, options), { expires: new Date(1970, 1, 1, 0, 0, 1), maxAge: 0 }));
        this.cookies = __assign$2({}, this.cookies);
        delete this.cookies[name];
        if (this.HAS_DOCUMENT_COOKIE) {
            document.cookie = serialize_1(name, '', finalOptions);
        }
        this._emitChange({ name: name, value: undefined, options: options });
    };
    Cookies.prototype.addChangeListener = function (callback) {
        this.changeListeners.push(callback);
    };
    Cookies.prototype.removeChangeListener = function (callback) {
        var idx = this.changeListeners.indexOf(callback);
        if (idx >= 0) {
            this.changeListeners.splice(idx, 1);
        }
    };
    return Cookies;
}());
var Cookies$1 = Cookies;

var isBrowser$1 = browserOrNode().isBrowser;
var UniversalStorage = /** @class */ (function () {
    function UniversalStorage(context) {
        if (context === void 0) { context = {}; }
        this.cookies = new Cookies$1();
        this.store = isBrowser$1 ? window.localStorage : Object.create(null);
        this.cookies = context.req
            ? new Cookies$1(context.req.headers.cookie)
            : new Cookies$1();
        Object.assign(this.store, this.cookies.getAll());
    }
    Object.defineProperty(UniversalStorage.prototype, "length", {
        get: function () {
            return Object.entries(this.store).length;
        },
        enumerable: true,
        configurable: true
    });
    UniversalStorage.prototype.clear = function () {
        var _this = this;
        Array.from(new Array(this.length))
            .map(function (_, i) { return _this.key(i); })
            .forEach(function (key) { return _this.removeItem(key); });
    };
    UniversalStorage.prototype.getItem = function (key) {
        return this.getLocalItem(key);
    };
    UniversalStorage.prototype.getLocalItem = function (key) {
        return Object.prototype.hasOwnProperty.call(this.store, key)
            ? this.store[key]
            : null;
    };
    UniversalStorage.prototype.getUniversalItem = function (key) {
        return this.cookies.get(key);
    };
    UniversalStorage.prototype.key = function (index) {
        return Object.keys(this.store)[index];
    };
    UniversalStorage.prototype.removeItem = function (key) {
        this.removeLocalItem(key);
        this.removeUniversalItem(key);
    };
    UniversalStorage.prototype.removeLocalItem = function (key) {
        delete this.store[key];
    };
    UniversalStorage.prototype.removeUniversalItem = function (key) {
        this.cookies.remove(key, {
            path: '/',
        });
    };
    UniversalStorage.prototype.setItem = function (key, value) {
        this.setLocalItem(key, value);
        // keys take the shape:
        //  1. `${ProviderPrefix}.${userPoolClientId}.${username}.${tokenType}
        //  2. `${ProviderPrefix}.${userPoolClientId}.LastAuthUser
        var tokenType = key.split('.').pop();
        switch (tokenType) {
            // LastAuthUser is needed for computing other key names
            case 'LastAuthUser':
            // accessToken is required for CognitoUserSession
            case 'accessToken':
            // refreshToken originates on the client, but SSR pages won't fail when this expires
            // Note: the new `accessToken` will also be refreshed on the client (since Amplify doesn't set server-side cookies)
            case 'refreshToken':
            // Required for CognitoUserSession
            case 'idToken':
                isBrowser$1
                    ? this.setUniversalItem(key, value)
                    : this.setLocalItem(key, value);
            // userData is used when `Auth.currentAuthenticatedUser({ bypassCache: false })`.
            // Can be persisted to speed up calls to `Auth.currentAuthenticatedUser()`
            // case 'userData':
            // Ignoring clockDrift on the server for now, but needs testing
            // case 'clockDrift':
        }
    };
    UniversalStorage.prototype.setLocalItem = function (key, value) {
        this.store[key] = value;
    };
    UniversalStorage.prototype.setUniversalItem = function (key, value) {
        this.cookies.set(key, value, {
            path: '/',
            // `httpOnly` cannot be set via JavaScript: https://developer.mozilla.org/en-US/docs/Web/HTTP/Cookies#JavaScript_access_using_Document.cookie
            sameSite: true,
            // Allow unsecure requests to http://localhost:3000/ when in development.
            secure: window.location.hostname === 'localhost' ? false : true,
        });
    };
    return UniversalStorage;
}());

/*
 * Copyright 2017-2017 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"). You may not use this file except in compliance with
 * the License. A copy of the License is located at
 *
 *     http://aws.amazon.com/apache2.0/
 *
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
 * and limitations under the License.
 */
({
    userAgent: Platform$1.userAgent,
});

/*!
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: Apache-2.0
 */

/** @class */
var AuthenticationDetails = /*#__PURE__*/function () {
  /**
   * Constructs a new AuthenticationDetails object
   * @param {object=} data Creation options.
   * @param {string} data.Username User being authenticated.
   * @param {string} data.Password Plain-text password to authenticate with.
   * @param {(AttributeArg[])?} data.ValidationData Application extra metadata.
   * @param {(AttributeArg[])?} data.AuthParamaters Authentication paramaters for custom auth.
   */
  function AuthenticationDetails(data) {
    var _ref = data || {},
        ValidationData = _ref.ValidationData,
        Username = _ref.Username,
        Password = _ref.Password,
        AuthParameters = _ref.AuthParameters,
        ClientMetadata = _ref.ClientMetadata;

    this.validationData = ValidationData || {};
    this.authParameters = AuthParameters || {};
    this.clientMetadata = ClientMetadata || {};
    this.username = Username;
    this.password = Password;
  }
  /**
   * @returns {string} the record's username
   */


  var _proto = AuthenticationDetails.prototype;

  _proto.getUsername = function getUsername() {
    return this.username;
  }
  /**
   * @returns {string} the record's password
   */
  ;

  _proto.getPassword = function getPassword() {
    return this.password;
  }
  /**
   * @returns {Array} the record's validationData
   */
  ;

  _proto.getValidationData = function getValidationData() {
    return this.validationData;
  }
  /**
   * @returns {Array} the record's authParameters
   */
  ;

  _proto.getAuthParameters = function getAuthParameters() {
    return this.authParameters;
  }
  /**
   * @returns {ClientMetadata} the clientMetadata for a Lambda trigger
   */
  ;

  _proto.getClientMetadata = function getClientMetadata() {
    return this.clientMetadata;
  };

  return AuthenticationDetails;
}();

var _nodeResolve_empty = {};

var _nodeResolve_empty$1 = /*#__PURE__*/Object.freeze({
    __proto__: null,
    'default': _nodeResolve_empty
});

var require$$0$1 = /*@__PURE__*/getDefaultExportFromNamespaceIfNotNamed(_nodeResolve_empty$1);

var core = createCommonjsModule(function (module, exports) {
(function (root, factory) {
	{
		// CommonJS
		module.exports = factory();
	}
}(commonjsGlobal, function () {

	/*globals window, global, require*/

	/**
	 * CryptoJS core components.
	 */
	var CryptoJS = CryptoJS || (function (Math, undefined$1) {

	    var crypto;

	    // Native crypto from window (Browser)
	    if (typeof window !== 'undefined' && window.crypto) {
	        crypto = window.crypto;
	    }

	    // Native crypto in web worker (Browser)
	    if (typeof self !== 'undefined' && self.crypto) {
	        crypto = self.crypto;
	    }

	    // Native crypto from worker
	    if (typeof globalThis !== 'undefined' && globalThis.crypto) {
	        crypto = globalThis.crypto;
	    }

	    // Native (experimental IE 11) crypto from window (Browser)
	    if (!crypto && typeof window !== 'undefined' && window.msCrypto) {
	        crypto = window.msCrypto;
	    }

	    // Native crypto from global (NodeJS)
	    if (!crypto && typeof commonjsGlobal !== 'undefined' && commonjsGlobal.crypto) {
	        crypto = commonjsGlobal.crypto;
	    }

	    // Native crypto import via require (NodeJS)
	    if (!crypto && typeof commonjsRequire === 'function') {
	        try {
	            crypto = require$$0$1;
	        } catch (err) {}
	    }

	    /*
	     * Cryptographically secure pseudorandom number generator
	     *
	     * As Math.random() is cryptographically not safe to use
	     */
	    var cryptoSecureRandomInt = function () {
	        if (crypto) {
	            // Use getRandomValues method (Browser)
	            if (typeof crypto.getRandomValues === 'function') {
	                try {
	                    return crypto.getRandomValues(new Uint32Array(1))[0];
	                } catch (err) {}
	            }

	            // Use randomBytes method (NodeJS)
	            if (typeof crypto.randomBytes === 'function') {
	                try {
	                    return crypto.randomBytes(4).readInt32LE();
	                } catch (err) {}
	            }
	        }

	        throw new Error('Native crypto module could not be used to get secure random number.');
	    };

	    /*
	     * Local polyfill of Object.create

	     */
	    var create = Object.create || (function () {
	        function F() {}

	        return function (obj) {
	            var subtype;

	            F.prototype = obj;

	            subtype = new F();

	            F.prototype = null;

	            return subtype;
	        };
	    }());

	    /**
	     * CryptoJS namespace.
	     */
	    var C = {};

	    /**
	     * Library namespace.
	     */
	    var C_lib = C.lib = {};

	    /**
	     * Base object for prototypal inheritance.
	     */
	    var Base = C_lib.Base = (function () {


	        return {
	            /**
	             * Creates a new object that inherits from this object.
	             *
	             * @param {Object} overrides Properties to copy into the new object.
	             *
	             * @return {Object} The new object.
	             *
	             * @static
	             *
	             * @example
	             *
	             *     var MyType = CryptoJS.lib.Base.extend({
	             *         field: 'value',
	             *
	             *         method: function () {
	             *         }
	             *     });
	             */
	            extend: function (overrides) {
	                // Spawn
	                var subtype = create(this);

	                // Augment
	                if (overrides) {
	                    subtype.mixIn(overrides);
	                }

	                // Create default initializer
	                if (!subtype.hasOwnProperty('init') || this.init === subtype.init) {
	                    subtype.init = function () {
	                        subtype.$super.init.apply(this, arguments);
	                    };
	                }

	                // Initializer's prototype is the subtype object
	                subtype.init.prototype = subtype;

	                // Reference supertype
	                subtype.$super = this;

	                return subtype;
	            },

	            /**
	             * Extends this object and runs the init method.
	             * Arguments to create() will be passed to init().
	             *
	             * @return {Object} The new object.
	             *
	             * @static
	             *
	             * @example
	             *
	             *     var instance = MyType.create();
	             */
	            create: function () {
	                var instance = this.extend();
	                instance.init.apply(instance, arguments);

	                return instance;
	            },

	            /**
	             * Initializes a newly created object.
	             * Override this method to add some logic when your objects are created.
	             *
	             * @example
	             *
	             *     var MyType = CryptoJS.lib.Base.extend({
	             *         init: function () {
	             *             // ...
	             *         }
	             *     });
	             */
	            init: function () {
	            },

	            /**
	             * Copies properties into this object.
	             *
	             * @param {Object} properties The properties to mix in.
	             *
	             * @example
	             *
	             *     MyType.mixIn({
	             *         field: 'value'
	             *     });
	             */
	            mixIn: function (properties) {
	                for (var propertyName in properties) {
	                    if (properties.hasOwnProperty(propertyName)) {
	                        this[propertyName] = properties[propertyName];
	                    }
	                }

	                // IE won't copy toString using the loop above
	                if (properties.hasOwnProperty('toString')) {
	                    this.toString = properties.toString;
	                }
	            },

	            /**
	             * Creates a copy of this object.
	             *
	             * @return {Object} The clone.
	             *
	             * @example
	             *
	             *     var clone = instance.clone();
	             */
	            clone: function () {
	                return this.init.prototype.extend(this);
	            }
	        };
	    }());

	    /**
	     * An array of 32-bit words.
	     *
	     * @property {Array} words The array of 32-bit words.
	     * @property {number} sigBytes The number of significant bytes in this word array.
	     */
	    var WordArray = C_lib.WordArray = Base.extend({
	        /**
	         * Initializes a newly created word array.
	         *
	         * @param {Array} words (Optional) An array of 32-bit words.
	         * @param {number} sigBytes (Optional) The number of significant bytes in the words.
	         *
	         * @example
	         *
	         *     var wordArray = CryptoJS.lib.WordArray.create();
	         *     var wordArray = CryptoJS.lib.WordArray.create([0x00010203, 0x04050607]);
	         *     var wordArray = CryptoJS.lib.WordArray.create([0x00010203, 0x04050607], 6);
	         */
	        init: function (words, sigBytes) {
	            words = this.words = words || [];

	            if (sigBytes != undefined$1) {
	                this.sigBytes = sigBytes;
	            } else {
	                this.sigBytes = words.length * 4;
	            }
	        },

	        /**
	         * Converts this word array to a string.
	         *
	         * @param {Encoder} encoder (Optional) The encoding strategy to use. Default: CryptoJS.enc.Hex
	         *
	         * @return {string} The stringified word array.
	         *
	         * @example
	         *
	         *     var string = wordArray + '';
	         *     var string = wordArray.toString();
	         *     var string = wordArray.toString(CryptoJS.enc.Utf8);
	         */
	        toString: function (encoder) {
	            return (encoder || Hex).stringify(this);
	        },

	        /**
	         * Concatenates a word array to this word array.
	         *
	         * @param {WordArray} wordArray The word array to append.
	         *
	         * @return {WordArray} This word array.
	         *
	         * @example
	         *
	         *     wordArray1.concat(wordArray2);
	         */
	        concat: function (wordArray) {
	            // Shortcuts
	            var thisWords = this.words;
	            var thatWords = wordArray.words;
	            var thisSigBytes = this.sigBytes;
	            var thatSigBytes = wordArray.sigBytes;

	            // Clamp excess bits
	            this.clamp();

	            // Concat
	            if (thisSigBytes % 4) {
	                // Copy one byte at a time
	                for (var i = 0; i < thatSigBytes; i++) {
	                    var thatByte = (thatWords[i >>> 2] >>> (24 - (i % 4) * 8)) & 0xff;
	                    thisWords[(thisSigBytes + i) >>> 2] |= thatByte << (24 - ((thisSigBytes + i) % 4) * 8);
	                }
	            } else {
	                // Copy one word at a time
	                for (var j = 0; j < thatSigBytes; j += 4) {
	                    thisWords[(thisSigBytes + j) >>> 2] = thatWords[j >>> 2];
	                }
	            }
	            this.sigBytes += thatSigBytes;

	            // Chainable
	            return this;
	        },

	        /**
	         * Removes insignificant bits.
	         *
	         * @example
	         *
	         *     wordArray.clamp();
	         */
	        clamp: function () {
	            // Shortcuts
	            var words = this.words;
	            var sigBytes = this.sigBytes;

	            // Clamp
	            words[sigBytes >>> 2] &= 0xffffffff << (32 - (sigBytes % 4) * 8);
	            words.length = Math.ceil(sigBytes / 4);
	        },

	        /**
	         * Creates a copy of this word array.
	         *
	         * @return {WordArray} The clone.
	         *
	         * @example
	         *
	         *     var clone = wordArray.clone();
	         */
	        clone: function () {
	            var clone = Base.clone.call(this);
	            clone.words = this.words.slice(0);

	            return clone;
	        },

	        /**
	         * Creates a word array filled with random bytes.
	         *
	         * @param {number} nBytes The number of random bytes to generate.
	         *
	         * @return {WordArray} The random word array.
	         *
	         * @static
	         *
	         * @example
	         *
	         *     var wordArray = CryptoJS.lib.WordArray.random(16);
	         */
	        random: function (nBytes) {
	            var words = [];

	            for (var i = 0; i < nBytes; i += 4) {
	                words.push(cryptoSecureRandomInt());
	            }

	            return new WordArray.init(words, nBytes);
	        }
	    });

	    /**
	     * Encoder namespace.
	     */
	    var C_enc = C.enc = {};

	    /**
	     * Hex encoding strategy.
	     */
	    var Hex = C_enc.Hex = {
	        /**
	         * Converts a word array to a hex string.
	         *
	         * @param {WordArray} wordArray The word array.
	         *
	         * @return {string} The hex string.
	         *
	         * @static
	         *
	         * @example
	         *
	         *     var hexString = CryptoJS.enc.Hex.stringify(wordArray);
	         */
	        stringify: function (wordArray) {
	            // Shortcuts
	            var words = wordArray.words;
	            var sigBytes = wordArray.sigBytes;

	            // Convert
	            var hexChars = [];
	            for (var i = 0; i < sigBytes; i++) {
	                var bite = (words[i >>> 2] >>> (24 - (i % 4) * 8)) & 0xff;
	                hexChars.push((bite >>> 4).toString(16));
	                hexChars.push((bite & 0x0f).toString(16));
	            }

	            return hexChars.join('');
	        },

	        /**
	         * Converts a hex string to a word array.
	         *
	         * @param {string} hexStr The hex string.
	         *
	         * @return {WordArray} The word array.
	         *
	         * @static
	         *
	         * @example
	         *
	         *     var wordArray = CryptoJS.enc.Hex.parse(hexString);
	         */
	        parse: function (hexStr) {
	            // Shortcut
	            var hexStrLength = hexStr.length;

	            // Convert
	            var words = [];
	            for (var i = 0; i < hexStrLength; i += 2) {
	                words[i >>> 3] |= parseInt(hexStr.substr(i, 2), 16) << (24 - (i % 8) * 4);
	            }

	            return new WordArray.init(words, hexStrLength / 2);
	        }
	    };

	    /**
	     * Latin1 encoding strategy.
	     */
	    var Latin1 = C_enc.Latin1 = {
	        /**
	         * Converts a word array to a Latin1 string.
	         *
	         * @param {WordArray} wordArray The word array.
	         *
	         * @return {string} The Latin1 string.
	         *
	         * @static
	         *
	         * @example
	         *
	         *     var latin1String = CryptoJS.enc.Latin1.stringify(wordArray);
	         */
	        stringify: function (wordArray) {
	            // Shortcuts
	            var words = wordArray.words;
	            var sigBytes = wordArray.sigBytes;

	            // Convert
	            var latin1Chars = [];
	            for (var i = 0; i < sigBytes; i++) {
	                var bite = (words[i >>> 2] >>> (24 - (i % 4) * 8)) & 0xff;
	                latin1Chars.push(String.fromCharCode(bite));
	            }

	            return latin1Chars.join('');
	        },

	        /**
	         * Converts a Latin1 string to a word array.
	         *
	         * @param {string} latin1Str The Latin1 string.
	         *
	         * @return {WordArray} The word array.
	         *
	         * @static
	         *
	         * @example
	         *
	         *     var wordArray = CryptoJS.enc.Latin1.parse(latin1String);
	         */
	        parse: function (latin1Str) {
	            // Shortcut
	            var latin1StrLength = latin1Str.length;

	            // Convert
	            var words = [];
	            for (var i = 0; i < latin1StrLength; i++) {
	                words[i >>> 2] |= (latin1Str.charCodeAt(i) & 0xff) << (24 - (i % 4) * 8);
	            }

	            return new WordArray.init(words, latin1StrLength);
	        }
	    };

	    /**
	     * UTF-8 encoding strategy.
	     */
	    var Utf8 = C_enc.Utf8 = {
	        /**
	         * Converts a word array to a UTF-8 string.
	         *
	         * @param {WordArray} wordArray The word array.
	         *
	         * @return {string} The UTF-8 string.
	         *
	         * @static
	         *
	         * @example
	         *
	         *     var utf8String = CryptoJS.enc.Utf8.stringify(wordArray);
	         */
	        stringify: function (wordArray) {
	            try {
	                return decodeURIComponent(escape(Latin1.stringify(wordArray)));
	            } catch (e) {
	                throw new Error('Malformed UTF-8 data');
	            }
	        },

	        /**
	         * Converts a UTF-8 string to a word array.
	         *
	         * @param {string} utf8Str The UTF-8 string.
	         *
	         * @return {WordArray} The word array.
	         *
	         * @static
	         *
	         * @example
	         *
	         *     var wordArray = CryptoJS.enc.Utf8.parse(utf8String);
	         */
	        parse: function (utf8Str) {
	            return Latin1.parse(unescape(encodeURIComponent(utf8Str)));
	        }
	    };

	    /**
	     * Abstract buffered block algorithm template.
	     *
	     * The property blockSize must be implemented in a concrete subtype.
	     *
	     * @property {number} _minBufferSize The number of blocks that should be kept unprocessed in the buffer. Default: 0
	     */
	    var BufferedBlockAlgorithm = C_lib.BufferedBlockAlgorithm = Base.extend({
	        /**
	         * Resets this block algorithm's data buffer to its initial state.
	         *
	         * @example
	         *
	         *     bufferedBlockAlgorithm.reset();
	         */
	        reset: function () {
	            // Initial values
	            this._data = new WordArray.init();
	            this._nDataBytes = 0;
	        },

	        /**
	         * Adds new data to this block algorithm's buffer.
	         *
	         * @param {WordArray|string} data The data to append. Strings are converted to a WordArray using UTF-8.
	         *
	         * @example
	         *
	         *     bufferedBlockAlgorithm._append('data');
	         *     bufferedBlockAlgorithm._append(wordArray);
	         */
	        _append: function (data) {
	            // Convert string to WordArray, else assume WordArray already
	            if (typeof data == 'string') {
	                data = Utf8.parse(data);
	            }

	            // Append
	            this._data.concat(data);
	            this._nDataBytes += data.sigBytes;
	        },

	        /**
	         * Processes available data blocks.
	         *
	         * This method invokes _doProcessBlock(offset), which must be implemented by a concrete subtype.
	         *
	         * @param {boolean} doFlush Whether all blocks and partial blocks should be processed.
	         *
	         * @return {WordArray} The processed data.
	         *
	         * @example
	         *
	         *     var processedData = bufferedBlockAlgorithm._process();
	         *     var processedData = bufferedBlockAlgorithm._process(!!'flush');
	         */
	        _process: function (doFlush) {
	            var processedWords;

	            // Shortcuts
	            var data = this._data;
	            var dataWords = data.words;
	            var dataSigBytes = data.sigBytes;
	            var blockSize = this.blockSize;
	            var blockSizeBytes = blockSize * 4;

	            // Count blocks ready
	            var nBlocksReady = dataSigBytes / blockSizeBytes;
	            if (doFlush) {
	                // Round up to include partial blocks
	                nBlocksReady = Math.ceil(nBlocksReady);
	            } else {
	                // Round down to include only full blocks,
	                // less the number of blocks that must remain in the buffer
	                nBlocksReady = Math.max((nBlocksReady | 0) - this._minBufferSize, 0);
	            }

	            // Count words ready
	            var nWordsReady = nBlocksReady * blockSize;

	            // Count bytes ready
	            var nBytesReady = Math.min(nWordsReady * 4, dataSigBytes);

	            // Process blocks
	            if (nWordsReady) {
	                for (var offset = 0; offset < nWordsReady; offset += blockSize) {
	                    // Perform concrete-algorithm logic
	                    this._doProcessBlock(dataWords, offset);
	                }

	                // Remove processed words
	                processedWords = dataWords.splice(0, nWordsReady);
	                data.sigBytes -= nBytesReady;
	            }

	            // Return processed words
	            return new WordArray.init(processedWords, nBytesReady);
	        },

	        /**
	         * Creates a copy of this object.
	         *
	         * @return {Object} The clone.
	         *
	         * @example
	         *
	         *     var clone = bufferedBlockAlgorithm.clone();
	         */
	        clone: function () {
	            var clone = Base.clone.call(this);
	            clone._data = this._data.clone();

	            return clone;
	        },

	        _minBufferSize: 0
	    });

	    /**
	     * Abstract hasher template.
	     *
	     * @property {number} blockSize The number of 32-bit words this hasher operates on. Default: 16 (512 bits)
	     */
	    C_lib.Hasher = BufferedBlockAlgorithm.extend({
	        /**
	         * Configuration options.
	         */
	        cfg: Base.extend(),

	        /**
	         * Initializes a newly created hasher.
	         *
	         * @param {Object} cfg (Optional) The configuration options to use for this hash computation.
	         *
	         * @example
	         *
	         *     var hasher = CryptoJS.algo.SHA256.create();
	         */
	        init: function (cfg) {
	            // Apply config defaults
	            this.cfg = this.cfg.extend(cfg);

	            // Set initial values
	            this.reset();
	        },

	        /**
	         * Resets this hasher to its initial state.
	         *
	         * @example
	         *
	         *     hasher.reset();
	         */
	        reset: function () {
	            // Reset data buffer
	            BufferedBlockAlgorithm.reset.call(this);

	            // Perform concrete-hasher logic
	            this._doReset();
	        },

	        /**
	         * Updates this hasher with a message.
	         *
	         * @param {WordArray|string} messageUpdate The message to append.
	         *
	         * @return {Hasher} This hasher.
	         *
	         * @example
	         *
	         *     hasher.update('message');
	         *     hasher.update(wordArray);
	         */
	        update: function (messageUpdate) {
	            // Append
	            this._append(messageUpdate);

	            // Update the hash
	            this._process();

	            // Chainable
	            return this;
	        },

	        /**
	         * Finalizes the hash computation.
	         * Note that the finalize operation is effectively a destructive, read-once operation.
	         *
	         * @param {WordArray|string} messageUpdate (Optional) A final message update.
	         *
	         * @return {WordArray} The hash.
	         *
	         * @example
	         *
	         *     var hash = hasher.finalize();
	         *     var hash = hasher.finalize('message');
	         *     var hash = hasher.finalize(wordArray);
	         */
	        finalize: function (messageUpdate) {
	            // Final message update
	            if (messageUpdate) {
	                this._append(messageUpdate);
	            }

	            // Perform concrete-hasher logic
	            var hash = this._doFinalize();

	            return hash;
	        },

	        blockSize: 512/32,

	        /**
	         * Creates a shortcut function to a hasher's object interface.
	         *
	         * @param {Hasher} hasher The hasher to create a helper for.
	         *
	         * @return {Function} The shortcut function.
	         *
	         * @static
	         *
	         * @example
	         *
	         *     var SHA256 = CryptoJS.lib.Hasher._createHelper(CryptoJS.algo.SHA256);
	         */
	        _createHelper: function (hasher) {
	            return function (message, cfg) {
	                return new hasher.init(cfg).finalize(message);
	            };
	        },

	        /**
	         * Creates a shortcut function to the HMAC's object interface.
	         *
	         * @param {Hasher} hasher The hasher to use in this HMAC helper.
	         *
	         * @return {Function} The shortcut function.
	         *
	         * @static
	         *
	         * @example
	         *
	         *     var HmacSHA256 = CryptoJS.lib.Hasher._createHmacHelper(CryptoJS.algo.SHA256);
	         */
	        _createHmacHelper: function (hasher) {
	            return function (message, key) {
	                return new C_algo.HMAC.init(hasher, key).finalize(message);
	            };
	        }
	    });

	    /**
	     * Algorithm namespace.
	     */
	    var C_algo = C.algo = {};

	    return C;
	}(Math));


	return CryptoJS;

}));
});

createCommonjsModule(function (module, exports) {
(function (root, factory) {
	{
		// CommonJS
		module.exports = factory(core);
	}
}(commonjsGlobal, function (CryptoJS) {

	(function () {
	    // Check if typed arrays are supported
	    if (typeof ArrayBuffer != 'function') {
	        return;
	    }

	    // Shortcuts
	    var C = CryptoJS;
	    var C_lib = C.lib;
	    var WordArray = C_lib.WordArray;

	    // Reference original init
	    var superInit = WordArray.init;

	    // Augment WordArray.init to handle typed arrays
	    var subInit = WordArray.init = function (typedArray) {
	        // Convert buffers to uint8
	        if (typedArray instanceof ArrayBuffer) {
	            typedArray = new Uint8Array(typedArray);
	        }

	        // Convert other array views to uint8
	        if (
	            typedArray instanceof Int8Array ||
	            (typeof Uint8ClampedArray !== "undefined" && typedArray instanceof Uint8ClampedArray) ||
	            typedArray instanceof Int16Array ||
	            typedArray instanceof Uint16Array ||
	            typedArray instanceof Int32Array ||
	            typedArray instanceof Uint32Array ||
	            typedArray instanceof Float32Array ||
	            typedArray instanceof Float64Array
	        ) {
	            typedArray = new Uint8Array(typedArray.buffer, typedArray.byteOffset, typedArray.byteLength);
	        }

	        // Handle Uint8Array
	        if (typedArray instanceof Uint8Array) {
	            // Shortcut
	            var typedArrayByteLength = typedArray.byteLength;

	            // Extract bytes
	            var words = [];
	            for (var i = 0; i < typedArrayByteLength; i++) {
	                words[i >>> 2] |= typedArray[i] << (24 - (i % 4) * 8);
	            }

	            // Initialize this word array
	            superInit.call(this, words, typedArrayByteLength);
	        } else {
	            // Else call normal init
	            superInit.apply(this, arguments);
	        }
	    };

	    subInit.prototype = WordArray;
	}());


	return CryptoJS.lib.WordArray;

}));
});

var sha256 = createCommonjsModule(function (module, exports) {
(function (root, factory) {
	{
		// CommonJS
		module.exports = factory(core);
	}
}(commonjsGlobal, function (CryptoJS) {

	(function (Math) {
	    // Shortcuts
	    var C = CryptoJS;
	    var C_lib = C.lib;
	    var WordArray = C_lib.WordArray;
	    var Hasher = C_lib.Hasher;
	    var C_algo = C.algo;

	    // Initialization and round constants tables
	    var H = [];
	    var K = [];

	    // Compute constants
	    (function () {
	        function isPrime(n) {
	            var sqrtN = Math.sqrt(n);
	            for (var factor = 2; factor <= sqrtN; factor++) {
	                if (!(n % factor)) {
	                    return false;
	                }
	            }

	            return true;
	        }

	        function getFractionalBits(n) {
	            return ((n - (n | 0)) * 0x100000000) | 0;
	        }

	        var n = 2;
	        var nPrime = 0;
	        while (nPrime < 64) {
	            if (isPrime(n)) {
	                if (nPrime < 8) {
	                    H[nPrime] = getFractionalBits(Math.pow(n, 1 / 2));
	                }
	                K[nPrime] = getFractionalBits(Math.pow(n, 1 / 3));

	                nPrime++;
	            }

	            n++;
	        }
	    }());

	    // Reusable object
	    var W = [];

	    /**
	     * SHA-256 hash algorithm.
	     */
	    var SHA256 = C_algo.SHA256 = Hasher.extend({
	        _doReset: function () {
	            this._hash = new WordArray.init(H.slice(0));
	        },

	        _doProcessBlock: function (M, offset) {
	            // Shortcut
	            var H = this._hash.words;

	            // Working variables
	            var a = H[0];
	            var b = H[1];
	            var c = H[2];
	            var d = H[3];
	            var e = H[4];
	            var f = H[5];
	            var g = H[6];
	            var h = H[7];

	            // Computation
	            for (var i = 0; i < 64; i++) {
	                if (i < 16) {
	                    W[i] = M[offset + i] | 0;
	                } else {
	                    var gamma0x = W[i - 15];
	                    var gamma0  = ((gamma0x << 25) | (gamma0x >>> 7))  ^
	                                  ((gamma0x << 14) | (gamma0x >>> 18)) ^
	                                   (gamma0x >>> 3);

	                    var gamma1x = W[i - 2];
	                    var gamma1  = ((gamma1x << 15) | (gamma1x >>> 17)) ^
	                                  ((gamma1x << 13) | (gamma1x >>> 19)) ^
	                                   (gamma1x >>> 10);

	                    W[i] = gamma0 + W[i - 7] + gamma1 + W[i - 16];
	                }

	                var ch  = (e & f) ^ (~e & g);
	                var maj = (a & b) ^ (a & c) ^ (b & c);

	                var sigma0 = ((a << 30) | (a >>> 2)) ^ ((a << 19) | (a >>> 13)) ^ ((a << 10) | (a >>> 22));
	                var sigma1 = ((e << 26) | (e >>> 6)) ^ ((e << 21) | (e >>> 11)) ^ ((e << 7)  | (e >>> 25));

	                var t1 = h + sigma1 + ch + K[i] + W[i];
	                var t2 = sigma0 + maj;

	                h = g;
	                g = f;
	                f = e;
	                e = (d + t1) | 0;
	                d = c;
	                c = b;
	                b = a;
	                a = (t1 + t2) | 0;
	            }

	            // Intermediate hash value
	            H[0] = (H[0] + a) | 0;
	            H[1] = (H[1] + b) | 0;
	            H[2] = (H[2] + c) | 0;
	            H[3] = (H[3] + d) | 0;
	            H[4] = (H[4] + e) | 0;
	            H[5] = (H[5] + f) | 0;
	            H[6] = (H[6] + g) | 0;
	            H[7] = (H[7] + h) | 0;
	        },

	        _doFinalize: function () {
	            // Shortcuts
	            var data = this._data;
	            var dataWords = data.words;

	            var nBitsTotal = this._nDataBytes * 8;
	            var nBitsLeft = data.sigBytes * 8;

	            // Add padding
	            dataWords[nBitsLeft >>> 5] |= 0x80 << (24 - nBitsLeft % 32);
	            dataWords[(((nBitsLeft + 64) >>> 9) << 4) + 14] = Math.floor(nBitsTotal / 0x100000000);
	            dataWords[(((nBitsLeft + 64) >>> 9) << 4) + 15] = nBitsTotal;
	            data.sigBytes = dataWords.length * 4;

	            // Hash final blocks
	            this._process();

	            // Return final computed hash
	            return this._hash;
	        },

	        clone: function () {
	            var clone = Hasher.clone.call(this);
	            clone._hash = this._hash.clone();

	            return clone;
	        }
	    });

	    /**
	     * Shortcut function to the hasher's object interface.
	     *
	     * @param {WordArray|string} message The message to hash.
	     *
	     * @return {WordArray} The hash.
	     *
	     * @static
	     *
	     * @example
	     *
	     *     var hash = CryptoJS.SHA256('message');
	     *     var hash = CryptoJS.SHA256(wordArray);
	     */
	    C.SHA256 = Hasher._createHelper(SHA256);

	    /**
	     * Shortcut function to the HMAC's object interface.
	     *
	     * @param {WordArray|string} message The message to hash.
	     * @param {WordArray|string} key The secret key.
	     *
	     * @return {WordArray} The HMAC.
	     *
	     * @static
	     *
	     * @example
	     *
	     *     var hmac = CryptoJS.HmacSHA256(message, key);
	     */
	    C.HmacSHA256 = Hasher._createHmacHelper(SHA256);
	}(Math));


	return CryptoJS.SHA256;

}));
});

var hmac = createCommonjsModule(function (module, exports) {
(function (root, factory) {
	{
		// CommonJS
		module.exports = factory(core);
	}
}(commonjsGlobal, function (CryptoJS) {

	(function () {
	    // Shortcuts
	    var C = CryptoJS;
	    var C_lib = C.lib;
	    var Base = C_lib.Base;
	    var C_enc = C.enc;
	    var Utf8 = C_enc.Utf8;
	    var C_algo = C.algo;

	    /**
	     * HMAC algorithm.
	     */
	    C_algo.HMAC = Base.extend({
	        /**
	         * Initializes a newly created HMAC.
	         *
	         * @param {Hasher} hasher The hash algorithm to use.
	         * @param {WordArray|string} key The secret key.
	         *
	         * @example
	         *
	         *     var hmacHasher = CryptoJS.algo.HMAC.create(CryptoJS.algo.SHA256, key);
	         */
	        init: function (hasher, key) {
	            // Init hasher
	            hasher = this._hasher = new hasher.init();

	            // Convert string to WordArray, else assume WordArray already
	            if (typeof key == 'string') {
	                key = Utf8.parse(key);
	            }

	            // Shortcuts
	            var hasherBlockSize = hasher.blockSize;
	            var hasherBlockSizeBytes = hasherBlockSize * 4;

	            // Allow arbitrary length keys
	            if (key.sigBytes > hasherBlockSizeBytes) {
	                key = hasher.finalize(key);
	            }

	            // Clamp excess bits
	            key.clamp();

	            // Clone key for inner and outer pads
	            var oKey = this._oKey = key.clone();
	            var iKey = this._iKey = key.clone();

	            // Shortcuts
	            var oKeyWords = oKey.words;
	            var iKeyWords = iKey.words;

	            // XOR keys with pad constants
	            for (var i = 0; i < hasherBlockSize; i++) {
	                oKeyWords[i] ^= 0x5c5c5c5c;
	                iKeyWords[i] ^= 0x36363636;
	            }
	            oKey.sigBytes = iKey.sigBytes = hasherBlockSizeBytes;

	            // Set initial values
	            this.reset();
	        },

	        /**
	         * Resets this HMAC to its initial state.
	         *
	         * @example
	         *
	         *     hmacHasher.reset();
	         */
	        reset: function () {
	            // Shortcut
	            var hasher = this._hasher;

	            // Reset
	            hasher.reset();
	            hasher.update(this._iKey);
	        },

	        /**
	         * Updates this HMAC with a message.
	         *
	         * @param {WordArray|string} messageUpdate The message to append.
	         *
	         * @return {HMAC} This HMAC instance.
	         *
	         * @example
	         *
	         *     hmacHasher.update('message');
	         *     hmacHasher.update(wordArray);
	         */
	        update: function (messageUpdate) {
	            this._hasher.update(messageUpdate);

	            // Chainable
	            return this;
	        },

	        /**
	         * Finalizes the HMAC computation.
	         * Note that the finalize operation is effectively a destructive, read-once operation.
	         *
	         * @param {WordArray|string} messageUpdate (Optional) A final message update.
	         *
	         * @return {WordArray} The HMAC.
	         *
	         * @example
	         *
	         *     var hmac = hmacHasher.finalize();
	         *     var hmac = hmacHasher.finalize('message');
	         *     var hmac = hmacHasher.finalize(wordArray);
	         */
	        finalize: function (messageUpdate) {
	            // Shortcut
	            var hasher = this._hasher;

	            // Compute HMAC
	            var innerHash = hasher.finalize(messageUpdate);
	            hasher.reset();
	            var hmac = hasher.finalize(this._oKey.clone().concat(innerHash));

	            return hmac;
	        }
	    });
	}());


}));
});

var hmacSha256 = createCommonjsModule(function (module, exports) {
(function (root, factory, undef) {
	{
		// CommonJS
		module.exports = factory(core, sha256, hmac);
	}
}(commonjsGlobal, function (CryptoJS) {

	return CryptoJS.HmacSHA256;

}));
});

var crypto$1; // Native crypto from window (Browser)

if (typeof window !== 'undefined' && window.crypto) {
  crypto$1 = window.crypto;
} // Native (experimental IE 11) crypto from window (Browser)


if (!crypto$1 && typeof window !== 'undefined' && window.msCrypto) {
  crypto$1 = window.msCrypto;
} // Native crypto from global (NodeJS)


if (!crypto$1 && typeof global$1 !== 'undefined' && global$1.crypto) {
  crypto$1 = global$1.crypto;
} // Native crypto import via require (NodeJS)


if (!crypto$1 && typeof require === 'function') {
  try {
    crypto$1 = require('crypto');
  } catch (err) {}
}
/*
 * Cryptographically secure pseudorandom number generator
 * As Math.random() is cryptographically not safe to use
 */


function cryptoSecureRandomInt() {
  if (crypto$1) {
    // Use getRandomValues method (Browser)
    if (typeof crypto$1.getRandomValues === 'function') {
      try {
        return crypto$1.getRandomValues(new Uint32Array(1))[0];
      } catch (err) {}
    } // Use randomBytes method (NodeJS)


    if (typeof crypto$1.randomBytes === 'function') {
      try {
        return crypto$1.randomBytes(4).readInt32LE();
      } catch (err) {}
    }
  }

  throw new Error('Native crypto module could not be used to get secure random number.');
}

/**
 * Hex encoding strategy.
 * Converts a word array to a hex string.
 * @param {WordArray} wordArray The word array.
 * @return {string} The hex string.
 * @static
 */

function hexStringify(wordArray) {
  // Shortcuts
  var words = wordArray.words;
  var sigBytes = wordArray.sigBytes; // Convert

  var hexChars = [];

  for (var i = 0; i < sigBytes; i++) {
    var bite = words[i >>> 2] >>> 24 - i % 4 * 8 & 0xff;
    hexChars.push((bite >>> 4).toString(16));
    hexChars.push((bite & 0x0f).toString(16));
  }

  return hexChars.join('');
}

var WordArray = /*#__PURE__*/function () {
  function WordArray(words, sigBytes) {
    words = this.words = words || [];

    if (sigBytes != undefined) {
      this.sigBytes = sigBytes;
    } else {
      this.sigBytes = words.length * 4;
    }
  }

  var _proto = WordArray.prototype;

  _proto.random = function random(nBytes) {
    var words = [];

    for (var i = 0; i < nBytes; i += 4) {
      words.push(cryptoSecureRandomInt());
    }

    return new WordArray(words, nBytes);
  };

  _proto.toString = function toString() {
    return hexStringify(this);
  };

  return WordArray;
}();

// A small implementation of BigInteger based on http://www-cs-students.stanford.edu/~tjw/jsbn/
/*
 * Copyright (c) 2003-2005  Tom Wu
 * All Rights Reserved.
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS-IS" AND WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS, IMPLIED OR OTHERWISE, INCLUDING WITHOUT LIMITATION, ANY
 * WARRANTY OF MERCHANTABILITY OR FITNESS FOR A PARTICULAR PURPOSE.
 *
 * IN NO EVENT SHALL TOM WU BE LIABLE FOR ANY SPECIAL, INCIDENTAL,
 * INDIRECT OR CONSEQUENTIAL DAMAGES OF ANY KIND, OR ANY DAMAGES WHATSOEVER
 * RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER OR NOT ADVISED OF
 * THE POSSIBILITY OF DAMAGE, AND ON ANY THEORY OF LIABILITY, ARISING OUT
 * OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 *
 * In addition, the following condition applies:
 *
 * All redistributions must retain an intact copy of this copyright notice
 * and disclaimer.
 */
// (public) Constructor

function BigInteger(a, b) {
  if (a != null) this.fromString(a, b);
} // return new, unset BigInteger


function nbi() {
  return new BigInteger(null);
} // Bits per digit


var dbits; // JavaScript engine analysis

var canary = 0xdeadbeefcafe;
var j_lm = (canary & 0xffffff) == 0xefcafe; // am: Compute w_j += (x*this_i), propagate carries,
// c is initial carry, returns final carry.
// c < 3*dvalue, x < 2*dvalue, this_i < dvalue
// We need to select the fastest one that works in this environment.
// am1: use a single mult and divide to get the high bits,
// max digit bits should be 26 because
// max internal value = 2*dvalue^2-2*dvalue (< 2^53)

function am1(i, x, w, j, c, n) {
  while (--n >= 0) {
    var v = x * this[i++] + w[j] + c;
    c = Math.floor(v / 0x4000000);
    w[j++] = v & 0x3ffffff;
  }

  return c;
} // am2 avoids a big mult-and-extract completely.
// Max digit bits should be <= 30 because we do bitwise ops
// on values up to 2*hdvalue^2-hdvalue-1 (< 2^31)


function am2(i, x, w, j, c, n) {
  var xl = x & 0x7fff,
      xh = x >> 15;

  while (--n >= 0) {
    var l = this[i] & 0x7fff;
    var h = this[i++] >> 15;
    var m = xh * l + h * xl;
    l = xl * l + ((m & 0x7fff) << 15) + w[j] + (c & 0x3fffffff);
    c = (l >>> 30) + (m >>> 15) + xh * h + (c >>> 30);
    w[j++] = l & 0x3fffffff;
  }

  return c;
} // Alternately, set max digit bits to 28 since some
// browsers slow down when dealing with 32-bit numbers.


function am3(i, x, w, j, c, n) {
  var xl = x & 0x3fff,
      xh = x >> 14;

  while (--n >= 0) {
    var l = this[i] & 0x3fff;
    var h = this[i++] >> 14;
    var m = xh * l + h * xl;
    l = xl * l + ((m & 0x3fff) << 14) + w[j] + c;
    c = (l >> 28) + (m >> 14) + xh * h;
    w[j++] = l & 0xfffffff;
  }

  return c;
}

var inBrowser = typeof navigator !== 'undefined';

if (inBrowser && j_lm && navigator.appName == 'Microsoft Internet Explorer') {
  BigInteger.prototype.am = am2;
  dbits = 30;
} else if (inBrowser && j_lm && navigator.appName != 'Netscape') {
  BigInteger.prototype.am = am1;
  dbits = 26;
} else {
  // Mozilla/Netscape seems to prefer am3
  BigInteger.prototype.am = am3;
  dbits = 28;
}

BigInteger.prototype.DB = dbits;
BigInteger.prototype.DM = (1 << dbits) - 1;
BigInteger.prototype.DV = 1 << dbits;
var BI_FP = 52;
BigInteger.prototype.FV = Math.pow(2, BI_FP);
BigInteger.prototype.F1 = BI_FP - dbits;
BigInteger.prototype.F2 = 2 * dbits - BI_FP; // Digit conversions

var BI_RM = '0123456789abcdefghijklmnopqrstuvwxyz';
var BI_RC = new Array();
var rr, vv;
rr = '0'.charCodeAt(0);

for (vv = 0; vv <= 9; ++vv) {
  BI_RC[rr++] = vv;
}

rr = 'a'.charCodeAt(0);

for (vv = 10; vv < 36; ++vv) {
  BI_RC[rr++] = vv;
}

rr = 'A'.charCodeAt(0);

for (vv = 10; vv < 36; ++vv) {
  BI_RC[rr++] = vv;
}

function int2char(n) {
  return BI_RM.charAt(n);
}

function intAt(s, i) {
  var c = BI_RC[s.charCodeAt(i)];
  return c == null ? -1 : c;
} // (protected) copy this to r


function bnpCopyTo(r) {
  for (var i = this.t - 1; i >= 0; --i) {
    r[i] = this[i];
  }

  r.t = this.t;
  r.s = this.s;
} // (protected) set from integer value x, -DV <= x < DV


function bnpFromInt(x) {
  this.t = 1;
  this.s = x < 0 ? -1 : 0;
  if (x > 0) this[0] = x;else if (x < -1) this[0] = x + this.DV;else this.t = 0;
} // return bigint initialized to value


function nbv(i) {
  var r = nbi();
  r.fromInt(i);
  return r;
} // (protected) set from string and radix


function bnpFromString(s, b) {
  var k;
  if (b == 16) k = 4;else if (b == 8) k = 3;else if (b == 2) k = 1;else if (b == 32) k = 5;else if (b == 4) k = 2;else throw new Error('Only radix 2, 4, 8, 16, 32 are supported');
  this.t = 0;
  this.s = 0;
  var i = s.length,
      mi = false,
      sh = 0;

  while (--i >= 0) {
    var x = intAt(s, i);

    if (x < 0) {
      if (s.charAt(i) == '-') mi = true;
      continue;
    }

    mi = false;
    if (sh == 0) this[this.t++] = x;else if (sh + k > this.DB) {
      this[this.t - 1] |= (x & (1 << this.DB - sh) - 1) << sh;
      this[this.t++] = x >> this.DB - sh;
    } else this[this.t - 1] |= x << sh;
    sh += k;
    if (sh >= this.DB) sh -= this.DB;
  }

  this.clamp();
  if (mi) BigInteger.ZERO.subTo(this, this);
} // (protected) clamp off excess high words


function bnpClamp() {
  var c = this.s & this.DM;

  while (this.t > 0 && this[this.t - 1] == c) {
    --this.t;
  }
} // (public) return string representation in given radix


function bnToString(b) {
  if (this.s < 0) return '-' + this.negate().toString(b);
  var k;
  if (b == 16) k = 4;else if (b == 8) k = 3;else if (b == 2) k = 1;else if (b == 32) k = 5;else if (b == 4) k = 2;else throw new Error('Only radix 2, 4, 8, 16, 32 are supported');
  var km = (1 << k) - 1,
      d,
      m = false,
      r = '',
      i = this.t;
  var p = this.DB - i * this.DB % k;

  if (i-- > 0) {
    if (p < this.DB && (d = this[i] >> p) > 0) {
      m = true;
      r = int2char(d);
    }

    while (i >= 0) {
      if (p < k) {
        d = (this[i] & (1 << p) - 1) << k - p;
        d |= this[--i] >> (p += this.DB - k);
      } else {
        d = this[i] >> (p -= k) & km;

        if (p <= 0) {
          p += this.DB;
          --i;
        }
      }

      if (d > 0) m = true;
      if (m) r += int2char(d);
    }
  }

  return m ? r : '0';
} // (public) -this


function bnNegate() {
  var r = nbi();
  BigInteger.ZERO.subTo(this, r);
  return r;
} // (public) |this|


function bnAbs() {
  return this.s < 0 ? this.negate() : this;
} // (public) return + if this > a, - if this < a, 0 if equal


function bnCompareTo(a) {
  var r = this.s - a.s;
  if (r != 0) return r;
  var i = this.t;
  r = i - a.t;
  if (r != 0) return this.s < 0 ? -r : r;

  while (--i >= 0) {
    if ((r = this[i] - a[i]) != 0) return r;
  }

  return 0;
} // returns bit length of the integer x


function nbits(x) {
  var r = 1,
      t;

  if ((t = x >>> 16) != 0) {
    x = t;
    r += 16;
  }

  if ((t = x >> 8) != 0) {
    x = t;
    r += 8;
  }

  if ((t = x >> 4) != 0) {
    x = t;
    r += 4;
  }

  if ((t = x >> 2) != 0) {
    x = t;
    r += 2;
  }

  if ((t = x >> 1) != 0) {
    x = t;
    r += 1;
  }

  return r;
} // (public) return the number of bits in "this"


function bnBitLength() {
  if (this.t <= 0) return 0;
  return this.DB * (this.t - 1) + nbits(this[this.t - 1] ^ this.s & this.DM);
} // (protected) r = this << n*DB


function bnpDLShiftTo(n, r) {
  var i;

  for (i = this.t - 1; i >= 0; --i) {
    r[i + n] = this[i];
  }

  for (i = n - 1; i >= 0; --i) {
    r[i] = 0;
  }

  r.t = this.t + n;
  r.s = this.s;
} // (protected) r = this >> n*DB


function bnpDRShiftTo(n, r) {
  for (var i = n; i < this.t; ++i) {
    r[i - n] = this[i];
  }

  r.t = Math.max(this.t - n, 0);
  r.s = this.s;
} // (protected) r = this << n


function bnpLShiftTo(n, r) {
  var bs = n % this.DB;
  var cbs = this.DB - bs;
  var bm = (1 << cbs) - 1;
  var ds = Math.floor(n / this.DB),
      c = this.s << bs & this.DM,
      i;

  for (i = this.t - 1; i >= 0; --i) {
    r[i + ds + 1] = this[i] >> cbs | c;
    c = (this[i] & bm) << bs;
  }

  for (i = ds - 1; i >= 0; --i) {
    r[i] = 0;
  }

  r[ds] = c;
  r.t = this.t + ds + 1;
  r.s = this.s;
  r.clamp();
} // (protected) r = this >> n


function bnpRShiftTo(n, r) {
  r.s = this.s;
  var ds = Math.floor(n / this.DB);

  if (ds >= this.t) {
    r.t = 0;
    return;
  }

  var bs = n % this.DB;
  var cbs = this.DB - bs;
  var bm = (1 << bs) - 1;
  r[0] = this[ds] >> bs;

  for (var i = ds + 1; i < this.t; ++i) {
    r[i - ds - 1] |= (this[i] & bm) << cbs;
    r[i - ds] = this[i] >> bs;
  }

  if (bs > 0) r[this.t - ds - 1] |= (this.s & bm) << cbs;
  r.t = this.t - ds;
  r.clamp();
} // (protected) r = this - a


function bnpSubTo(a, r) {
  var i = 0,
      c = 0,
      m = Math.min(a.t, this.t);

  while (i < m) {
    c += this[i] - a[i];
    r[i++] = c & this.DM;
    c >>= this.DB;
  }

  if (a.t < this.t) {
    c -= a.s;

    while (i < this.t) {
      c += this[i];
      r[i++] = c & this.DM;
      c >>= this.DB;
    }

    c += this.s;
  } else {
    c += this.s;

    while (i < a.t) {
      c -= a[i];
      r[i++] = c & this.DM;
      c >>= this.DB;
    }

    c -= a.s;
  }

  r.s = c < 0 ? -1 : 0;
  if (c < -1) r[i++] = this.DV + c;else if (c > 0) r[i++] = c;
  r.t = i;
  r.clamp();
} // (protected) r = this * a, r != this,a (HAC 14.12)
// "this" should be the larger one if appropriate.


function bnpMultiplyTo(a, r) {
  var x = this.abs(),
      y = a.abs();
  var i = x.t;
  r.t = i + y.t;

  while (--i >= 0) {
    r[i] = 0;
  }

  for (i = 0; i < y.t; ++i) {
    r[i + x.t] = x.am(0, y[i], r, i, 0, x.t);
  }

  r.s = 0;
  r.clamp();
  if (this.s != a.s) BigInteger.ZERO.subTo(r, r);
} // (protected) r = this^2, r != this (HAC 14.16)


function bnpSquareTo(r) {
  var x = this.abs();
  var i = r.t = 2 * x.t;

  while (--i >= 0) {
    r[i] = 0;
  }

  for (i = 0; i < x.t - 1; ++i) {
    var c = x.am(i, x[i], r, 2 * i, 0, 1);

    if ((r[i + x.t] += x.am(i + 1, 2 * x[i], r, 2 * i + 1, c, x.t - i - 1)) >= x.DV) {
      r[i + x.t] -= x.DV;
      r[i + x.t + 1] = 1;
    }
  }

  if (r.t > 0) r[r.t - 1] += x.am(i, x[i], r, 2 * i, 0, 1);
  r.s = 0;
  r.clamp();
} // (protected) divide this by m, quotient and remainder to q, r (HAC 14.20)
// r != q, this != m.  q or r may be null.


function bnpDivRemTo(m, q, r) {
  var pm = m.abs();
  if (pm.t <= 0) return;
  var pt = this.abs();

  if (pt.t < pm.t) {
    if (q != null) q.fromInt(0);
    if (r != null) this.copyTo(r);
    return;
  }

  if (r == null) r = nbi();
  var y = nbi(),
      ts = this.s,
      ms = m.s;
  var nsh = this.DB - nbits(pm[pm.t - 1]); // normalize modulus

  if (nsh > 0) {
    pm.lShiftTo(nsh, y);
    pt.lShiftTo(nsh, r);
  } else {
    pm.copyTo(y);
    pt.copyTo(r);
  }

  var ys = y.t;
  var y0 = y[ys - 1];
  if (y0 == 0) return;
  var yt = y0 * (1 << this.F1) + (ys > 1 ? y[ys - 2] >> this.F2 : 0);
  var d1 = this.FV / yt,
      d2 = (1 << this.F1) / yt,
      e = 1 << this.F2;
  var i = r.t,
      j = i - ys,
      t = q == null ? nbi() : q;
  y.dlShiftTo(j, t);

  if (r.compareTo(t) >= 0) {
    r[r.t++] = 1;
    r.subTo(t, r);
  }

  BigInteger.ONE.dlShiftTo(ys, t);
  t.subTo(y, y); // "negative" y so we can replace sub with am later

  while (y.t < ys) {
    y[y.t++] = 0;
  }

  while (--j >= 0) {
    // Estimate quotient digit
    var qd = r[--i] == y0 ? this.DM : Math.floor(r[i] * d1 + (r[i - 1] + e) * d2);

    if ((r[i] += y.am(0, qd, r, j, 0, ys)) < qd) {
      // Try it out
      y.dlShiftTo(j, t);
      r.subTo(t, r);

      while (r[i] < --qd) {
        r.subTo(t, r);
      }
    }
  }

  if (q != null) {
    r.drShiftTo(ys, q);
    if (ts != ms) BigInteger.ZERO.subTo(q, q);
  }

  r.t = ys;
  r.clamp();
  if (nsh > 0) r.rShiftTo(nsh, r); // Denormalize remainder

  if (ts < 0) BigInteger.ZERO.subTo(r, r);
} // (public) this mod a


function bnMod(a) {
  var r = nbi();
  this.abs().divRemTo(a, null, r);
  if (this.s < 0 && r.compareTo(BigInteger.ZERO) > 0) a.subTo(r, r);
  return r;
} // (protected) return "-1/this % 2^DB"; useful for Mont. reduction
// justification:
//         xy == 1 (mod m)
//         xy =  1+km
//   xy(2-xy) = (1+km)(1-km)
// x[y(2-xy)] = 1-k^2m^2
// x[y(2-xy)] == 1 (mod m^2)
// if y is 1/x mod m, then y(2-xy) is 1/x mod m^2
// should reduce x and y(2-xy) by m^2 at each step to keep size bounded.
// JS multiply "overflows" differently from C/C++, so care is needed here.


function bnpInvDigit() {
  if (this.t < 1) return 0;
  var x = this[0];
  if ((x & 1) == 0) return 0;
  var y = x & 3; // y == 1/x mod 2^2

  y = y * (2 - (x & 0xf) * y) & 0xf; // y == 1/x mod 2^4

  y = y * (2 - (x & 0xff) * y) & 0xff; // y == 1/x mod 2^8

  y = y * (2 - ((x & 0xffff) * y & 0xffff)) & 0xffff; // y == 1/x mod 2^16
  // last step - calculate inverse mod DV directly;
  // assumes 16 < DB <= 32 and assumes ability to handle 48-bit ints

  y = y * (2 - x * y % this.DV) % this.DV; // y == 1/x mod 2^dbits
  // we really want the negative inverse, and -DV < y < DV

  return y > 0 ? this.DV - y : -y;
}

function bnEquals(a) {
  return this.compareTo(a) == 0;
} // (protected) r = this + a


function bnpAddTo(a, r) {
  var i = 0,
      c = 0,
      m = Math.min(a.t, this.t);

  while (i < m) {
    c += this[i] + a[i];
    r[i++] = c & this.DM;
    c >>= this.DB;
  }

  if (a.t < this.t) {
    c += a.s;

    while (i < this.t) {
      c += this[i];
      r[i++] = c & this.DM;
      c >>= this.DB;
    }

    c += this.s;
  } else {
    c += this.s;

    while (i < a.t) {
      c += a[i];
      r[i++] = c & this.DM;
      c >>= this.DB;
    }

    c += a.s;
  }

  r.s = c < 0 ? -1 : 0;
  if (c > 0) r[i++] = c;else if (c < -1) r[i++] = this.DV + c;
  r.t = i;
  r.clamp();
} // (public) this + a


function bnAdd(a) {
  var r = nbi();
  this.addTo(a, r);
  return r;
} // (public) this - a


function bnSubtract(a) {
  var r = nbi();
  this.subTo(a, r);
  return r;
} // (public) this * a


function bnMultiply(a) {
  var r = nbi();
  this.multiplyTo(a, r);
  return r;
} // (public) this / a


function bnDivide(a) {
  var r = nbi();
  this.divRemTo(a, r, null);
  return r;
} // Montgomery reduction


function Montgomery(m) {
  this.m = m;
  this.mp = m.invDigit();
  this.mpl = this.mp & 0x7fff;
  this.mph = this.mp >> 15;
  this.um = (1 << m.DB - 15) - 1;
  this.mt2 = 2 * m.t;
} // xR mod m


function montConvert(x) {
  var r = nbi();
  x.abs().dlShiftTo(this.m.t, r);
  r.divRemTo(this.m, null, r);
  if (x.s < 0 && r.compareTo(BigInteger.ZERO) > 0) this.m.subTo(r, r);
  return r;
} // x/R mod m


function montRevert(x) {
  var r = nbi();
  x.copyTo(r);
  this.reduce(r);
  return r;
} // x = x/R mod m (HAC 14.32)


function montReduce(x) {
  while (x.t <= this.mt2) {
    // pad x so am has enough room later
    x[x.t++] = 0;
  }

  for (var i = 0; i < this.m.t; ++i) {
    // faster way of calculating u0 = x[i]*mp mod DV
    var j = x[i] & 0x7fff;
    var u0 = j * this.mpl + ((j * this.mph + (x[i] >> 15) * this.mpl & this.um) << 15) & x.DM; // use am to combine the multiply-shift-add into one call

    j = i + this.m.t;
    x[j] += this.m.am(0, u0, x, i, 0, this.m.t); // propagate carry

    while (x[j] >= x.DV) {
      x[j] -= x.DV;
      x[++j]++;
    }
  }

  x.clamp();
  x.drShiftTo(this.m.t, x);
  if (x.compareTo(this.m) >= 0) x.subTo(this.m, x);
} // r = "x^2/R mod m"; x != r


function montSqrTo(x, r) {
  x.squareTo(r);
  this.reduce(r);
} // r = "xy/R mod m"; x,y != r


function montMulTo(x, y, r) {
  x.multiplyTo(y, r);
  this.reduce(r);
}

Montgomery.prototype.convert = montConvert;
Montgomery.prototype.revert = montRevert;
Montgomery.prototype.reduce = montReduce;
Montgomery.prototype.mulTo = montMulTo;
Montgomery.prototype.sqrTo = montSqrTo; // (public) this^e % m (HAC 14.85)

function bnModPow(e, m, callback) {
  var i = e.bitLength(),
      k,
      r = nbv(1),
      z = new Montgomery(m);
  if (i <= 0) return r;else if (i < 18) k = 1;else if (i < 48) k = 3;else if (i < 144) k = 4;else if (i < 768) k = 5;else k = 6; // precomputation

  var g = new Array(),
      n = 3,
      k1 = k - 1,
      km = (1 << k) - 1;
  g[1] = z.convert(this);

  if (k > 1) {
    var g2 = nbi();
    z.sqrTo(g[1], g2);

    while (n <= km) {
      g[n] = nbi();
      z.mulTo(g2, g[n - 2], g[n]);
      n += 2;
    }
  }

  var j = e.t - 1,
      w,
      is1 = true,
      r2 = nbi(),
      t;
  i = nbits(e[j]) - 1;

  while (j >= 0) {
    if (i >= k1) w = e[j] >> i - k1 & km;else {
      w = (e[j] & (1 << i + 1) - 1) << k1 - i;
      if (j > 0) w |= e[j - 1] >> this.DB + i - k1;
    }
    n = k;

    while ((w & 1) == 0) {
      w >>= 1;
      --n;
    }

    if ((i -= n) < 0) {
      i += this.DB;
      --j;
    }

    if (is1) {
      // ret == 1, don't bother squaring or multiplying it
      g[w].copyTo(r);
      is1 = false;
    } else {
      while (n > 1) {
        z.sqrTo(r, r2);
        z.sqrTo(r2, r);
        n -= 2;
      }

      if (n > 0) z.sqrTo(r, r2);else {
        t = r;
        r = r2;
        r2 = t;
      }
      z.mulTo(r2, g[w], r);
    }

    while (j >= 0 && (e[j] & 1 << i) == 0) {
      z.sqrTo(r, r2);
      t = r;
      r = r2;
      r2 = t;

      if (--i < 0) {
        i = this.DB - 1;
        --j;
      }
    }
  }

  var result = z.revert(r);
  callback(null, result);
  return result;
} // protected


BigInteger.prototype.copyTo = bnpCopyTo;
BigInteger.prototype.fromInt = bnpFromInt;
BigInteger.prototype.fromString = bnpFromString;
BigInteger.prototype.clamp = bnpClamp;
BigInteger.prototype.dlShiftTo = bnpDLShiftTo;
BigInteger.prototype.drShiftTo = bnpDRShiftTo;
BigInteger.prototype.lShiftTo = bnpLShiftTo;
BigInteger.prototype.rShiftTo = bnpRShiftTo;
BigInteger.prototype.subTo = bnpSubTo;
BigInteger.prototype.multiplyTo = bnpMultiplyTo;
BigInteger.prototype.squareTo = bnpSquareTo;
BigInteger.prototype.divRemTo = bnpDivRemTo;
BigInteger.prototype.invDigit = bnpInvDigit;
BigInteger.prototype.addTo = bnpAddTo; // public

BigInteger.prototype.toString = bnToString;
BigInteger.prototype.negate = bnNegate;
BigInteger.prototype.abs = bnAbs;
BigInteger.prototype.compareTo = bnCompareTo;
BigInteger.prototype.bitLength = bnBitLength;
BigInteger.prototype.mod = bnMod;
BigInteger.prototype.equals = bnEquals;
BigInteger.prototype.add = bnAdd;
BigInteger.prototype.subtract = bnSubtract;
BigInteger.prototype.multiply = bnMultiply;
BigInteger.prototype.divide = bnDivide;
BigInteger.prototype.modPow = bnModPow; // "constants"

BigInteger.ZERO = nbv(0);
BigInteger.ONE = nbv(1);

/*!
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: Apache-2.0
 */
/**
 * Returns a Buffer with a sequence of random nBytes
 *
 * @param {number} nBytes
 * @returns {Buffer} fixed-length sequence of random bytes
 */

function randomBytes(nBytes) {
  return Buffer.from(new WordArray().random(nBytes).toString(), 'hex');
}
/**
 * Tests if a hex string has it most significant bit set (case-insensitive regex)
 */

var HEX_MSB_REGEX = /^[89a-f]/i;
var initN = 'FFFFFFFFFFFFFFFFC90FDAA22168C234C4C6628B80DC1CD1' + '29024E088A67CC74020BBEA63B139B22514A08798E3404DD' + 'EF9519B3CD3A431B302B0A6DF25F14374FE1356D6D51C245' + 'E485B576625E7EC6F44C42E9A637ED6B0BFF5CB6F406B7ED' + 'EE386BFB5A899FA5AE9F24117C4B1FE649286651ECE45B3D' + 'C2007CB8A163BF0598DA48361C55D39A69163FA8FD24CF5F' + '83655D23DCA3AD961C62F356208552BB9ED529077096966D' + '670C354E4ABC9804F1746C08CA18217C32905E462E36CE3B' + 'E39E772C180E86039B2783A2EC07A28FB5C55DF06F4C52C9' + 'DE2BCBF6955817183995497CEA956AE515D2261898FA0510' + '15728E5A8AAAC42DAD33170D04507A33A85521ABDF1CBA64' + 'ECFB850458DBEF0A8AEA71575D060C7DB3970F85A6E1E4C7' + 'ABF5AE8CDB0933D71E8C94E04A25619DCEE3D2261AD2EE6B' + 'F12FFA06D98A0864D87602733EC86A64521F2B18177B200C' + 'BBE117577A615D6C770988C0BAD946E208E24FA074E5AB31' + '43DB5BFCE0FD108E4B82D120A93AD2CAFFFFFFFFFFFFFFFF';
var newPasswordRequiredChallengeUserAttributePrefix = 'userAttributes.';
/** @class */

var AuthenticationHelper = /*#__PURE__*/function () {
  /**
   * Constructs a new AuthenticationHelper object
   * @param {string} PoolName Cognito user pool name.
   */
  function AuthenticationHelper(PoolName) {
    this.N = new BigInteger(initN, 16);
    this.g = new BigInteger('2', 16);
    this.k = new BigInteger(this.hexHash("" + this.padHex(this.N) + this.padHex(this.g)), 16);
    this.smallAValue = this.generateRandomSmallA();
    this.getLargeAValue(function () {});
    this.infoBits = Buffer.from('Caldera Derived Key', 'utf8');
    this.poolName = PoolName;
  }
  /**
   * @returns {BigInteger} small A, a random number
   */


  var _proto = AuthenticationHelper.prototype;

  _proto.getSmallAValue = function getSmallAValue() {
    return this.smallAValue;
  }
  /**
   * @param {nodeCallback<BigInteger>} callback Called with (err, largeAValue)
   * @returns {void}
   */
  ;

  _proto.getLargeAValue = function getLargeAValue(callback) {
    var _this = this;

    if (this.largeAValue) {
      callback(null, this.largeAValue);
    } else {
      this.calculateA(this.smallAValue, function (err, largeAValue) {
        if (err) {
          callback(err, null);
        }

        _this.largeAValue = largeAValue;
        callback(null, _this.largeAValue);
      });
    }
  }
  /**
   * helper function to generate a random big integer
   * @returns {BigInteger} a random value.
   * @private
   */
  ;

  _proto.generateRandomSmallA = function generateRandomSmallA() {
    // This will be interpreted as a postive 128-bit integer
    var hexRandom = randomBytes(128).toString('hex');
    var randomBigInt = new BigInteger(hexRandom, 16); // There is no need to do randomBigInt.mod(this.N - 1) as N (3072-bit) is > 128 bytes (1024-bit)

    return randomBigInt;
  }
  /**
   * helper function to generate a random string
   * @returns {string} a random value.
   * @private
   */
  ;

  _proto.generateRandomString = function generateRandomString() {
    return randomBytes(40).toString('base64');
  }
  /**
   * @returns {string} Generated random value included in password hash.
   */
  ;

  _proto.getRandomPassword = function getRandomPassword() {
    return this.randomPassword;
  }
  /**
   * @returns {string} Generated random value included in devices hash.
   */
  ;

  _proto.getSaltDevices = function getSaltDevices() {
    return this.SaltToHashDevices;
  }
  /**
   * @returns {string} Value used to verify devices.
   */
  ;

  _proto.getVerifierDevices = function getVerifierDevices() {
    return this.verifierDevices;
  }
  /**
   * Generate salts and compute verifier.
   * @param {string} deviceGroupKey Devices to generate verifier for.
   * @param {string} username User to generate verifier for.
   * @param {nodeCallback<null>} callback Called with (err, null)
   * @returns {void}
   */
  ;

  _proto.generateHashDevice = function generateHashDevice(deviceGroupKey, username, callback) {
    var _this2 = this;

    this.randomPassword = this.generateRandomString();
    var combinedString = "" + deviceGroupKey + username + ":" + this.randomPassword;
    var hashedString = this.hash(combinedString);
    var hexRandom = randomBytes(16).toString('hex'); // The random hex will be unambiguously represented as a postive integer

    this.SaltToHashDevices = this.padHex(new BigInteger(hexRandom, 16));
    this.g.modPow(new BigInteger(this.hexHash(this.SaltToHashDevices + hashedString), 16), this.N, function (err, verifierDevicesNotPadded) {
      if (err) {
        callback(err, null);
      }

      _this2.verifierDevices = _this2.padHex(verifierDevicesNotPadded);
      callback(null, null);
    });
  }
  /**
   * Calculate the client's public value A = g^a%N
   * with the generated random number a
   * @param {BigInteger} a Randomly generated small A.
   * @param {nodeCallback<BigInteger>} callback Called with (err, largeAValue)
   * @returns {void}
   * @private
   */
  ;

  _proto.calculateA = function calculateA(a, callback) {
    var _this3 = this;

    this.g.modPow(a, this.N, function (err, A) {
      if (err) {
        callback(err, null);
      }

      if (A.mod(_this3.N).equals(BigInteger.ZERO)) {
        callback(new Error('Illegal paramater. A mod N cannot be 0.'), null);
      }

      callback(null, A);
    });
  }
  /**
   * Calculate the client's value U which is the hash of A and B
   * @param {BigInteger} A Large A value.
   * @param {BigInteger} B Server B value.
   * @returns {BigInteger} Computed U value.
   * @private
   */
  ;

  _proto.calculateU = function calculateU(A, B) {
    this.UHexHash = this.hexHash(this.padHex(A) + this.padHex(B));
    var finalU = new BigInteger(this.UHexHash, 16);
    return finalU;
  }
  /**
   * Calculate a hash from a bitArray
   * @param {Buffer} buf Value to hash.
   * @returns {String} Hex-encoded hash.
   * @private
   */
  ;

  _proto.hash = function hash(buf) {
    var str = buf instanceof Buffer ? core.lib.WordArray.create(buf) : buf;
    var hashHex = sha256(str).toString();
    return new Array(64 - hashHex.length).join('0') + hashHex;
  }
  /**
   * Calculate a hash from a hex string
   * @param {String} hexStr Value to hash.
   * @returns {String} Hex-encoded hash.
   * @private
   */
  ;

  _proto.hexHash = function hexHash(hexStr) {
    return this.hash(Buffer.from(hexStr, 'hex'));
  }
  /**
   * Standard hkdf algorithm
   * @param {Buffer} ikm Input key material.
   * @param {Buffer} salt Salt value.
   * @returns {Buffer} Strong key material.
   * @private
   */
  ;

  _proto.computehkdf = function computehkdf(ikm, salt) {
    var infoBitsWordArray = core.lib.WordArray.create(Buffer.concat([this.infoBits, Buffer.from(String.fromCharCode(1), 'utf8')]));
    var ikmWordArray = ikm instanceof Buffer ? core.lib.WordArray.create(ikm) : ikm;
    var saltWordArray = salt instanceof Buffer ? core.lib.WordArray.create(salt) : salt;
    var prk = hmacSha256(ikmWordArray, saltWordArray);
    var hmac = hmacSha256(infoBitsWordArray, prk);
    return Buffer.from(hmac.toString(), 'hex').slice(0, 16);
  }
  /**
   * Calculates the final hkdf based on computed S value, and computed U value and the key
   * @param {String} username Username.
   * @param {String} password Password.
   * @param {BigInteger} serverBValue Server B value.
   * @param {BigInteger} salt Generated salt.
   * @param {nodeCallback<Buffer>} callback Called with (err, hkdfValue)
   * @returns {void}
   */
  ;

  _proto.getPasswordAuthenticationKey = function getPasswordAuthenticationKey(username, password, serverBValue, salt, callback) {
    var _this4 = this;

    if (serverBValue.mod(this.N).equals(BigInteger.ZERO)) {
      throw new Error('B cannot be zero.');
    }

    this.UValue = this.calculateU(this.largeAValue, serverBValue);

    if (this.UValue.equals(BigInteger.ZERO)) {
      throw new Error('U cannot be zero.');
    }

    var usernamePassword = "" + this.poolName + username + ":" + password;
    var usernamePasswordHash = this.hash(usernamePassword);
    var xValue = new BigInteger(this.hexHash(this.padHex(salt) + usernamePasswordHash), 16);
    this.calculateS(xValue, serverBValue, function (err, sValue) {
      if (err) {
        callback(err, null);
      }

      var hkdf = _this4.computehkdf(Buffer.from(_this4.padHex(sValue), 'hex'), Buffer.from(_this4.padHex(_this4.UValue), 'hex'));

      callback(null, hkdf);
    });
  }
  /**
   * Calculates the S value used in getPasswordAuthenticationKey
   * @param {BigInteger} xValue Salted password hash value.
   * @param {BigInteger} serverBValue Server B value.
   * @param {nodeCallback<string>} callback Called on success or error.
   * @returns {void}
   */
  ;

  _proto.calculateS = function calculateS(xValue, serverBValue, callback) {
    var _this5 = this;

    this.g.modPow(xValue, this.N, function (err, gModPowXN) {
      if (err) {
        callback(err, null);
      }

      var intValue2 = serverBValue.subtract(_this5.k.multiply(gModPowXN));
      intValue2.modPow(_this5.smallAValue.add(_this5.UValue.multiply(xValue)), _this5.N, function (err2, result) {
        if (err2) {
          callback(err2, null);
        }

        callback(null, result.mod(_this5.N));
      });
    });
  }
  /**
   * Return constant newPasswordRequiredChallengeUserAttributePrefix
   * @return {newPasswordRequiredChallengeUserAttributePrefix} constant prefix value
   */
  ;

  _proto.getNewPasswordRequiredChallengeUserAttributePrefix = function getNewPasswordRequiredChallengeUserAttributePrefix() {
    return newPasswordRequiredChallengeUserAttributePrefix;
  }
  /**
   * Returns an unambiguous, even-length hex string of the two's complement encoding of an integer.
   *
   * It is compatible with the hex encoding of Java's BigInteger's toByteArray(), wich returns a
   * byte array containing the two's-complement representation of a BigInteger. The array contains
   * the minimum number of bytes required to represent the BigInteger, including at least one sign bit.
   *
   * Examples showing how ambiguity is avoided by left padding with:
   * 	"00" (for positive values where the most-significant-bit is set)
   *  "FF" (for negative values where the most-significant-bit is set)
   *
   * padHex(bigInteger.fromInt(-236))  === "FF14"
   * padHex(bigInteger.fromInt(20))    === "14"
   *
   * padHex(bigInteger.fromInt(-200))  === "FF38"
   * padHex(bigInteger.fromInt(56))    === "38"
   *
   * padHex(bigInteger.fromInt(-20))   === "EC"
   * padHex(bigInteger.fromInt(236))   === "00EC"
   *
   * padHex(bigInteger.fromInt(-56))   === "C8"
   * padHex(bigInteger.fromInt(200))   === "00C8"
   *
   * @param {BigInteger} bigInt Number to encode.
   * @returns {String} even-length hex string of the two's complement encoding.
   */
  ;

  _proto.padHex = function padHex(bigInt) {
    if (!(bigInt instanceof BigInteger)) {
      throw new Error('Not a BigInteger');
    }

    var isNegative = bigInt.compareTo(BigInteger.ZERO) < 0;
    /* Get a hex string for abs(bigInt) */

    var hexStr = bigInt.abs().toString(16);
    /* Pad hex to even length if needed */

    hexStr = hexStr.length % 2 !== 0 ? "0" + hexStr : hexStr;
    /* Prepend "00" if the most significant bit is set */

    hexStr = HEX_MSB_REGEX.test(hexStr) ? "00" + hexStr : hexStr;

    if (isNegative) {
      /* Flip the bits of the representation */
      var invertedNibbles = hexStr.split('').map(function (x) {
        var invertedNibble = ~parseInt(x, 16) & 0xf;
        return '0123456789ABCDEF'.charAt(invertedNibble);
      }).join('');
      /* After flipping the bits, add one to get the 2's complement representation */

      var flippedBitsBI = new BigInteger(invertedNibbles, 16).add(BigInteger.ONE);
      hexStr = flippedBitsBI.toString(16);
      /*
      For hex strings starting with 'FF8', 'FF' can be dropped, e.g. 0xFFFF80=0xFF80=0x80=-128
      		Any sequence of '1' bits on the left can always be substituted with a single '1' bit
      without changing the represented value.
      		This only happens in the case when the input is 80...00
      */

      if (hexStr.toUpperCase().startsWith('FF8')) {
        hexStr = hexStr.substring(2);
      }
    }

    return hexStr;
  };

  return AuthenticationHelper;
}();

/*!
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: Apache-2.0
 */
/** @class */

var CognitoJwtToken = /*#__PURE__*/function () {
  /**
   * Constructs a new CognitoJwtToken object
   * @param {string=} token The JWT token.
   */
  function CognitoJwtToken(token) {
    // Assign object
    this.jwtToken = token || '';
    this.payload = this.decodePayload();
  }
  /**
   * @returns {string} the record's token.
   */


  var _proto = CognitoJwtToken.prototype;

  _proto.getJwtToken = function getJwtToken() {
    return this.jwtToken;
  }
  /**
   * @returns {int} the token's expiration (exp member).
   */
  ;

  _proto.getExpiration = function getExpiration() {
    return this.payload.exp;
  }
  /**
   * @returns {int} the token's "issued at" (iat member).
   */
  ;

  _proto.getIssuedAt = function getIssuedAt() {
    return this.payload.iat;
  }
  /**
   * @returns {object} the token's payload.
   */
  ;

  _proto.decodePayload = function decodePayload() {
    var payload = this.jwtToken.split('.')[1];

    try {
      return JSON.parse(Buffer.from(payload, 'base64').toString('utf8'));
    } catch (err) {
      return {};
    }
  };

  return CognitoJwtToken;
}();

function _inheritsLoose$2(subClass, superClass) { subClass.prototype = Object.create(superClass.prototype); subClass.prototype.constructor = subClass; _setPrototypeOf$2(subClass, superClass); }

function _setPrototypeOf$2(o, p) { _setPrototypeOf$2 = Object.setPrototypeOf || function _setPrototypeOf(o, p) { o.__proto__ = p; return o; }; return _setPrototypeOf$2(o, p); }
/** @class */

var CognitoAccessToken = /*#__PURE__*/function (_CognitoJwtToken) {
  _inheritsLoose$2(CognitoAccessToken, _CognitoJwtToken);

  /**
   * Constructs a new CognitoAccessToken object
   * @param {string=} AccessToken The JWT access token.
   */
  function CognitoAccessToken(_temp) {
    var _ref = _temp === void 0 ? {} : _temp,
        AccessToken = _ref.AccessToken;

    return _CognitoJwtToken.call(this, AccessToken || '') || this;
  }

  return CognitoAccessToken;
}(CognitoJwtToken);

function _inheritsLoose$1(subClass, superClass) { subClass.prototype = Object.create(superClass.prototype); subClass.prototype.constructor = subClass; _setPrototypeOf$1(subClass, superClass); }

function _setPrototypeOf$1(o, p) { _setPrototypeOf$1 = Object.setPrototypeOf || function _setPrototypeOf(o, p) { o.__proto__ = p; return o; }; return _setPrototypeOf$1(o, p); }
/** @class */

var CognitoIdToken = /*#__PURE__*/function (_CognitoJwtToken) {
  _inheritsLoose$1(CognitoIdToken, _CognitoJwtToken);

  /**
   * Constructs a new CognitoIdToken object
   * @param {string=} IdToken The JWT Id token
   */
  function CognitoIdToken(_temp) {
    var _ref = _temp === void 0 ? {} : _temp,
        IdToken = _ref.IdToken;

    return _CognitoJwtToken.call(this, IdToken || '') || this;
  }

  return CognitoIdToken;
}(CognitoJwtToken);

/*!
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: Apache-2.0
 */

/** @class */
var CognitoRefreshToken = /*#__PURE__*/function () {
  /**
   * Constructs a new CognitoRefreshToken object
   * @param {string=} RefreshToken The JWT refresh token.
   */
  function CognitoRefreshToken(_temp) {
    var _ref = _temp === void 0 ? {} : _temp,
        RefreshToken = _ref.RefreshToken;

    // Assign object
    this.token = RefreshToken || '';
  }
  /**
   * @returns {string} the record's token.
   */


  var _proto = CognitoRefreshToken.prototype;

  _proto.getToken = function getToken() {
    return this.token;
  };

  return CognitoRefreshToken;
}();

var encBase64 = createCommonjsModule(function (module, exports) {
(function (root, factory) {
	{
		// CommonJS
		module.exports = factory(core);
	}
}(commonjsGlobal, function (CryptoJS) {

	(function () {
	    // Shortcuts
	    var C = CryptoJS;
	    var C_lib = C.lib;
	    var WordArray = C_lib.WordArray;
	    var C_enc = C.enc;

	    /**
	     * Base64 encoding strategy.
	     */
	    C_enc.Base64 = {
	        /**
	         * Converts a word array to a Base64 string.
	         *
	         * @param {WordArray} wordArray The word array.
	         *
	         * @return {string} The Base64 string.
	         *
	         * @static
	         *
	         * @example
	         *
	         *     var base64String = CryptoJS.enc.Base64.stringify(wordArray);
	         */
	        stringify: function (wordArray) {
	            // Shortcuts
	            var words = wordArray.words;
	            var sigBytes = wordArray.sigBytes;
	            var map = this._map;

	            // Clamp excess bits
	            wordArray.clamp();

	            // Convert
	            var base64Chars = [];
	            for (var i = 0; i < sigBytes; i += 3) {
	                var byte1 = (words[i >>> 2]       >>> (24 - (i % 4) * 8))       & 0xff;
	                var byte2 = (words[(i + 1) >>> 2] >>> (24 - ((i + 1) % 4) * 8)) & 0xff;
	                var byte3 = (words[(i + 2) >>> 2] >>> (24 - ((i + 2) % 4) * 8)) & 0xff;

	                var triplet = (byte1 << 16) | (byte2 << 8) | byte3;

	                for (var j = 0; (j < 4) && (i + j * 0.75 < sigBytes); j++) {
	                    base64Chars.push(map.charAt((triplet >>> (6 * (3 - j))) & 0x3f));
	                }
	            }

	            // Add padding
	            var paddingChar = map.charAt(64);
	            if (paddingChar) {
	                while (base64Chars.length % 4) {
	                    base64Chars.push(paddingChar);
	                }
	            }

	            return base64Chars.join('');
	        },

	        /**
	         * Converts a Base64 string to a word array.
	         *
	         * @param {string} base64Str The Base64 string.
	         *
	         * @return {WordArray} The word array.
	         *
	         * @static
	         *
	         * @example
	         *
	         *     var wordArray = CryptoJS.enc.Base64.parse(base64String);
	         */
	        parse: function (base64Str) {
	            // Shortcuts
	            var base64StrLength = base64Str.length;
	            var map = this._map;
	            var reverseMap = this._reverseMap;

	            if (!reverseMap) {
	                    reverseMap = this._reverseMap = [];
	                    for (var j = 0; j < map.length; j++) {
	                        reverseMap[map.charCodeAt(j)] = j;
	                    }
	            }

	            // Ignore padding
	            var paddingChar = map.charAt(64);
	            if (paddingChar) {
	                var paddingIndex = base64Str.indexOf(paddingChar);
	                if (paddingIndex !== -1) {
	                    base64StrLength = paddingIndex;
	                }
	            }

	            // Convert
	            return parseLoop(base64Str, base64StrLength, reverseMap);

	        },

	        _map: 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/='
	    };

	    function parseLoop(base64Str, base64StrLength, reverseMap) {
	      var words = [];
	      var nBytes = 0;
	      for (var i = 0; i < base64StrLength; i++) {
	          if (i % 4) {
	              var bits1 = reverseMap[base64Str.charCodeAt(i - 1)] << ((i % 4) * 2);
	              var bits2 = reverseMap[base64Str.charCodeAt(i)] >>> (6 - (i % 4) * 2);
	              var bitsCombined = bits1 | bits2;
	              words[nBytes >>> 2] |= bitsCombined << (24 - (nBytes % 4) * 8);
	              nBytes++;
	          }
	      }
	      return WordArray.create(words, nBytes);
	    }
	}());


	return CryptoJS.enc.Base64;

}));
});

/*!
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: Apache-2.0
 */

/** @class */
var CognitoUserSession = /*#__PURE__*/function () {
  /**
   * Constructs a new CognitoUserSession object
   * @param {CognitoIdToken} IdToken The session's Id token.
   * @param {CognitoRefreshToken=} RefreshToken The session's refresh token.
   * @param {CognitoAccessToken} AccessToken The session's access token.
   * @param {int} ClockDrift The saved computer's clock drift or undefined to force calculation.
   */
  function CognitoUserSession(_temp) {
    var _ref = _temp === void 0 ? {} : _temp,
        IdToken = _ref.IdToken,
        RefreshToken = _ref.RefreshToken,
        AccessToken = _ref.AccessToken,
        ClockDrift = _ref.ClockDrift;

    if (AccessToken == null || IdToken == null) {
      throw new Error('Id token and Access Token must be present.');
    }

    this.idToken = IdToken;
    this.refreshToken = RefreshToken;
    this.accessToken = AccessToken;
    this.clockDrift = ClockDrift === undefined ? this.calculateClockDrift() : ClockDrift;
  }
  /**
   * @returns {CognitoIdToken} the session's Id token
   */


  var _proto = CognitoUserSession.prototype;

  _proto.getIdToken = function getIdToken() {
    return this.idToken;
  }
  /**
   * @returns {CognitoRefreshToken} the session's refresh token
   */
  ;

  _proto.getRefreshToken = function getRefreshToken() {
    return this.refreshToken;
  }
  /**
   * @returns {CognitoAccessToken} the session's access token
   */
  ;

  _proto.getAccessToken = function getAccessToken() {
    return this.accessToken;
  }
  /**
   * @returns {int} the session's clock drift
   */
  ;

  _proto.getClockDrift = function getClockDrift() {
    return this.clockDrift;
  }
  /**
   * @returns {int} the computer's clock drift
   */
  ;

  _proto.calculateClockDrift = function calculateClockDrift() {
    var now = Math.floor(new Date() / 1000);
    var iat = Math.min(this.accessToken.getIssuedAt(), this.idToken.getIssuedAt());
    return now - iat;
  }
  /**
   * Checks to see if the session is still valid based on session expiry information found
   * in tokens and the current time (adjusted with clock drift)
   * @returns {boolean} if the session is still valid
   */
  ;

  _proto.isValid = function isValid() {
    var now = Math.floor(new Date() / 1000);
    var adjusted = now - this.clockDrift;
    return adjusted < this.accessToken.getExpiration() && adjusted < this.idToken.getExpiration();
  };

  return CognitoUserSession;
}();

/*!
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: Apache-2.0
 */
var monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
var weekNames = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
/** @class */

var DateHelper = /*#__PURE__*/function () {
  function DateHelper() {}

  var _proto = DateHelper.prototype;

  /**
   * @returns {string} The current time in "ddd MMM D HH:mm:ss UTC YYYY" format.
   */
  _proto.getNowString = function getNowString() {
    var now = new Date();
    var weekDay = weekNames[now.getUTCDay()];
    var month = monthNames[now.getUTCMonth()];
    var day = now.getUTCDate();
    var hours = now.getUTCHours();

    if (hours < 10) {
      hours = "0" + hours;
    }

    var minutes = now.getUTCMinutes();

    if (minutes < 10) {
      minutes = "0" + minutes;
    }

    var seconds = now.getUTCSeconds();

    if (seconds < 10) {
      seconds = "0" + seconds;
    }

    var year = now.getUTCFullYear(); // ddd MMM D HH:mm:ss UTC YYYY

    var dateNow = weekDay + " " + month + " " + day + " " + hours + ":" + minutes + ":" + seconds + " UTC " + year;
    return dateNow;
  };

  return DateHelper;
}();

/*!
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: Apache-2.0
 */

/** @class */
var CognitoUserAttribute = /*#__PURE__*/function () {
  /**
   * Constructs a new CognitoUserAttribute object
   * @param {string=} Name The record's name
   * @param {string=} Value The record's value
   */
  function CognitoUserAttribute(_temp) {
    var _ref = _temp === void 0 ? {} : _temp,
        Name = _ref.Name,
        Value = _ref.Value;

    this.Name = Name || '';
    this.Value = Value || '';
  }
  /**
   * @returns {string} the record's value.
   */


  var _proto = CognitoUserAttribute.prototype;

  _proto.getValue = function getValue() {
    return this.Value;
  }
  /**
   * Sets the record's value.
   * @param {string} value The new value.
   * @returns {CognitoUserAttribute} The record for method chaining.
   */
  ;

  _proto.setValue = function setValue(value) {
    this.Value = value;
    return this;
  }
  /**
   * @returns {string} the record's name.
   */
  ;

  _proto.getName = function getName() {
    return this.Name;
  }
  /**
   * Sets the record's name
   * @param {string} name The new name.
   * @returns {CognitoUserAttribute} The record for method chaining.
   */
  ;

  _proto.setName = function setName(name) {
    this.Name = name;
    return this;
  }
  /**
   * @returns {string} a string representation of the record.
   */
  ;

  _proto.toString = function toString() {
    return JSON.stringify(this);
  }
  /**
   * @returns {object} a flat object representing the record.
   */
  ;

  _proto.toJSON = function toJSON() {
    return {
      Name: this.Name,
      Value: this.Value
    };
  };

  return CognitoUserAttribute;
}();

/*!
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: Apache-2.0
 */
var dataMemory = {};
/** @class */

var MemoryStorage = /*#__PURE__*/function () {
  function MemoryStorage() {}

  /**
   * This is used to set a specific item in storage
   * @param {string} key - the key for the item
   * @param {object} value - the value
   * @returns {string} value that was set
   */
  MemoryStorage.setItem = function setItem(key, value) {
    dataMemory[key] = value;
    return dataMemory[key];
  }
  /**
   * This is used to get a specific key from storage
   * @param {string} key - the key for the item
   * This is used to clear the storage
   * @returns {string} the data item
   */
  ;

  MemoryStorage.getItem = function getItem(key) {
    return Object.prototype.hasOwnProperty.call(dataMemory, key) ? dataMemory[key] : undefined;
  }
  /**
   * This is used to remove an item from storage
   * @param {string} key - the key being set
   * @returns {boolean} return true
   */
  ;

  MemoryStorage.removeItem = function removeItem(key) {
    return delete dataMemory[key];
  }
  /**
   * This is used to clear the storage
   * @returns {string} nothing
   */
  ;

  MemoryStorage.clear = function clear() {
    dataMemory = {};
    return dataMemory;
  };

  return MemoryStorage;
}();
/** @class */

var StorageHelper = /*#__PURE__*/function () {
  /**
   * This is used to get a storage object
   * @returns {object} the storage
   */
  function StorageHelper() {
    try {
      this.storageWindow = window.localStorage;
      this.storageWindow.setItem('aws.cognito.test-ls', 1);
      this.storageWindow.removeItem('aws.cognito.test-ls');
    } catch (exception) {
      this.storageWindow = MemoryStorage;
    }
  }
  /**
   * This is used to return the storage
   * @returns {object} the storage
   */


  var _proto = StorageHelper.prototype;

  _proto.getStorage = function getStorage() {
    return this.storageWindow;
  };

  return StorageHelper;
}();

/*!
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: Apache-2.0
 */
/**
 * @callback nodeCallback
 * @template T result
 * @param {*} err The operation failure reason, or null.
 * @param {T} result The operation result.
 */

/**
 * @callback onFailure
 * @param {*} err Failure reason.
 */

/**
 * @callback onSuccess
 * @template T result
 * @param {T} result The operation result.
 */

/**
 * @callback mfaRequired
 * @param {*} details MFA challenge details.
 */

/**
 * @callback customChallenge
 * @param {*} details Custom challenge details.
 */

/**
 * @callback inputVerificationCode
 * @param {*} data Server response.
 */

/**
 * @callback authSuccess
 * @param {CognitoUserSession} session The new session.
 * @param {bool=} userConfirmationNecessary User must be confirmed.
 */

var isBrowser = typeof navigator !== 'undefined';
var userAgent = isBrowser ? navigator.userAgent : 'nodejs';
/** @class */

var CognitoUser = /*#__PURE__*/function () {
  /**
   * Constructs a new CognitoUser object
   * @param {object} data Creation options
   * @param {string} data.Username The user's username.
   * @param {CognitoUserPool} data.Pool Pool containing the user.
   * @param {object} data.Storage Optional storage object.
   */
  function CognitoUser(data) {
    if (data == null || data.Username == null || data.Pool == null) {
      throw new Error('Username and Pool information are required.');
    }

    this.username = data.Username || '';
    this.pool = data.Pool;
    this.Session = null;
    this.client = data.Pool.client;
    this.signInUserSession = null;
    this.authenticationFlowType = 'USER_SRP_AUTH';
    this.storage = data.Storage || new StorageHelper().getStorage();
    this.keyPrefix = "CognitoIdentityServiceProvider." + this.pool.getClientId();
    this.userDataKey = this.keyPrefix + "." + this.username + ".userData";
  }
  /**
   * Sets the session for this user
   * @param {CognitoUserSession} signInUserSession the session
   * @returns {void}
   */


  var _proto = CognitoUser.prototype;

  _proto.setSignInUserSession = function setSignInUserSession(signInUserSession) {
    this.clearCachedUserData();
    this.signInUserSession = signInUserSession;
    this.cacheTokens();
  }
  /**
   * @returns {CognitoUserSession} the current session for this user
   */
  ;

  _proto.getSignInUserSession = function getSignInUserSession() {
    return this.signInUserSession;
  }
  /**
   * @returns {string} the user's username
   */
  ;

  _proto.getUsername = function getUsername() {
    return this.username;
  }
  /**
   * @returns {String} the authentication flow type
   */
  ;

  _proto.getAuthenticationFlowType = function getAuthenticationFlowType() {
    return this.authenticationFlowType;
  }
  /**
   * sets authentication flow type
   * @param {string} authenticationFlowType New value.
   * @returns {void}
   */
  ;

  _proto.setAuthenticationFlowType = function setAuthenticationFlowType(authenticationFlowType) {
    this.authenticationFlowType = authenticationFlowType;
  }
  /**
   * This is used for authenticating the user through the custom authentication flow.
   * @param {AuthenticationDetails} authDetails Contains the authentication data
   * @param {object} callback Result callback map.
   * @param {onFailure} callback.onFailure Called on any error.
   * @param {customChallenge} callback.customChallenge Custom challenge
   *        response required to continue.
   * @param {authSuccess} callback.onSuccess Called on success with the new session.
   * @returns {void}
   */
  ;

  _proto.initiateAuth = function initiateAuth(authDetails, callback) {
    var _this = this;

    var authParameters = authDetails.getAuthParameters();
    authParameters.USERNAME = this.username;
    var clientMetaData = Object.keys(authDetails.getValidationData()).length !== 0 ? authDetails.getValidationData() : authDetails.getClientMetadata();
    var jsonReq = {
      AuthFlow: 'CUSTOM_AUTH',
      ClientId: this.pool.getClientId(),
      AuthParameters: authParameters,
      ClientMetadata: clientMetaData
    };

    if (this.getUserContextData()) {
      jsonReq.UserContextData = this.getUserContextData();
    }

    this.client.request('InitiateAuth', jsonReq, function (err, data) {
      if (err) {
        return callback.onFailure(err);
      }

      var challengeName = data.ChallengeName;
      var challengeParameters = data.ChallengeParameters;

      if (challengeName === 'CUSTOM_CHALLENGE') {
        _this.Session = data.Session;
        return callback.customChallenge(challengeParameters);
      }

      _this.signInUserSession = _this.getCognitoUserSession(data.AuthenticationResult);

      _this.cacheTokens();

      return callback.onSuccess(_this.signInUserSession);
    });
  }
  /**
   * This is used for authenticating the user.
   * stuff
   * @param {AuthenticationDetails} authDetails Contains the authentication data
   * @param {object} callback Result callback map.
   * @param {onFailure} callback.onFailure Called on any error.
   * @param {newPasswordRequired} callback.newPasswordRequired new
   *        password and any required attributes are required to continue
   * @param {mfaRequired} callback.mfaRequired MFA code
   *        required to continue.
   * @param {customChallenge} callback.customChallenge Custom challenge
   *        response required to continue.
   * @param {authSuccess} callback.onSuccess Called on success with the new session.
   * @returns {void}
   */
  ;

  _proto.authenticateUser = function authenticateUser(authDetails, callback) {
    if (this.authenticationFlowType === 'USER_PASSWORD_AUTH') {
      return this.authenticateUserPlainUsernamePassword(authDetails, callback);
    } else if (this.authenticationFlowType === 'USER_SRP_AUTH' || this.authenticationFlowType === 'CUSTOM_AUTH') {
      return this.authenticateUserDefaultAuth(authDetails, callback);
    }

    return callback.onFailure(new Error('Authentication flow type is invalid.'));
  }
  /**
   * PRIVATE ONLY: This is an internal only method and should not
   * be directly called by the consumers.
   * It calls the AuthenticationHelper for SRP related
   * stuff
   * @param {AuthenticationDetails} authDetails Contains the authentication data
   * @param {object} callback Result callback map.
   * @param {onFailure} callback.onFailure Called on any error.
   * @param {newPasswordRequired} callback.newPasswordRequired new
   *        password and any required attributes are required to continue
   * @param {mfaRequired} callback.mfaRequired MFA code
   *        required to continue.
   * @param {customChallenge} callback.customChallenge Custom challenge
   *        response required to continue.
   * @param {authSuccess} callback.onSuccess Called on success with the new session.
   * @returns {void}
   */
  ;

  _proto.authenticateUserDefaultAuth = function authenticateUserDefaultAuth(authDetails, callback) {
    var _this2 = this;

    var authenticationHelper = new AuthenticationHelper(this.pool.getUserPoolId().split('_')[1]);
    var dateHelper = new DateHelper();
    var serverBValue;
    var salt;
    var authParameters = {};

    if (this.deviceKey != null) {
      authParameters.DEVICE_KEY = this.deviceKey;
    }

    authParameters.USERNAME = this.username;
    authenticationHelper.getLargeAValue(function (errOnAValue, aValue) {
      // getLargeAValue callback start
      if (errOnAValue) {
        callback.onFailure(errOnAValue);
      }

      authParameters.SRP_A = aValue.toString(16);

      if (_this2.authenticationFlowType === 'CUSTOM_AUTH') {
        authParameters.CHALLENGE_NAME = 'SRP_A';
      }

      var clientMetaData = Object.keys(authDetails.getValidationData()).length !== 0 ? authDetails.getValidationData() : authDetails.getClientMetadata();
      var jsonReq = {
        AuthFlow: _this2.authenticationFlowType,
        ClientId: _this2.pool.getClientId(),
        AuthParameters: authParameters,
        ClientMetadata: clientMetaData
      };

      if (_this2.getUserContextData(_this2.username)) {
        jsonReq.UserContextData = _this2.getUserContextData(_this2.username);
      }

      _this2.client.request('InitiateAuth', jsonReq, function (err, data) {
        if (err) {
          return callback.onFailure(err);
        }

        var challengeParameters = data.ChallengeParameters;
        _this2.username = challengeParameters.USER_ID_FOR_SRP;
        _this2.userDataKey = _this2.keyPrefix + "." + _this2.username + ".userData";
        serverBValue = new BigInteger(challengeParameters.SRP_B, 16);
        salt = new BigInteger(challengeParameters.SALT, 16);

        _this2.getCachedDeviceKeyAndPassword();

        authenticationHelper.getPasswordAuthenticationKey(_this2.username, authDetails.getPassword(), serverBValue, salt, function (errOnHkdf, hkdf) {
          // getPasswordAuthenticationKey callback start
          if (errOnHkdf) {
            callback.onFailure(errOnHkdf);
          }

          var dateNow = dateHelper.getNowString();
          var message = core.lib.WordArray.create(Buffer.concat([Buffer.from(_this2.pool.getUserPoolId().split('_')[1], 'utf8'), Buffer.from(_this2.username, 'utf8'), Buffer.from(challengeParameters.SECRET_BLOCK, 'base64'), Buffer.from(dateNow, 'utf8')]));
          var key = core.lib.WordArray.create(hkdf);
          var signatureString = encBase64.stringify(hmacSha256(message, key));
          var challengeResponses = {};
          challengeResponses.USERNAME = _this2.username;
          challengeResponses.PASSWORD_CLAIM_SECRET_BLOCK = challengeParameters.SECRET_BLOCK;
          challengeResponses.TIMESTAMP = dateNow;
          challengeResponses.PASSWORD_CLAIM_SIGNATURE = signatureString;

          if (_this2.deviceKey != null) {
            challengeResponses.DEVICE_KEY = _this2.deviceKey;
          }

          var respondToAuthChallenge = function respondToAuthChallenge(challenge, challengeCallback) {
            return _this2.client.request('RespondToAuthChallenge', challenge, function (errChallenge, dataChallenge) {
              if (errChallenge && errChallenge.code === 'ResourceNotFoundException' && errChallenge.message.toLowerCase().indexOf('device') !== -1) {
                challengeResponses.DEVICE_KEY = null;
                _this2.deviceKey = null;
                _this2.randomPassword = null;
                _this2.deviceGroupKey = null;

                _this2.clearCachedDeviceKeyAndPassword();

                return respondToAuthChallenge(challenge, challengeCallback);
              }

              return challengeCallback(errChallenge, dataChallenge);
            });
          };

          var jsonReqResp = {
            ChallengeName: 'PASSWORD_VERIFIER',
            ClientId: _this2.pool.getClientId(),
            ChallengeResponses: challengeResponses,
            Session: data.Session,
            ClientMetadata: clientMetaData
          };

          if (_this2.getUserContextData()) {
            jsonReqResp.UserContextData = _this2.getUserContextData();
          }

          respondToAuthChallenge(jsonReqResp, function (errAuthenticate, dataAuthenticate) {
            if (errAuthenticate) {
              return callback.onFailure(errAuthenticate);
            }

            return _this2.authenticateUserInternal(dataAuthenticate, authenticationHelper, callback);
          });
          return undefined; // getPasswordAuthenticationKey callback end
        });
        return undefined;
      }); // getLargeAValue callback end

    });
  }
  /**
   * PRIVATE ONLY: This is an internal only method and should not
   * be directly called by the consumers.
   * @param {AuthenticationDetails} authDetails Contains the authentication data.
   * @param {object} callback Result callback map.
   * @param {onFailure} callback.onFailure Called on any error.
   * @param {mfaRequired} callback.mfaRequired MFA code
   *        required to continue.
   * @param {authSuccess} callback.onSuccess Called on success with the new session.
   * @returns {void}
   */
  ;

  _proto.authenticateUserPlainUsernamePassword = function authenticateUserPlainUsernamePassword(authDetails, callback) {
    var _this3 = this;

    var authParameters = {};
    authParameters.USERNAME = this.username;
    authParameters.PASSWORD = authDetails.getPassword();

    if (!authParameters.PASSWORD) {
      callback.onFailure(new Error('PASSWORD parameter is required'));
      return;
    }

    var authenticationHelper = new AuthenticationHelper(this.pool.getUserPoolId().split('_')[1]);
    this.getCachedDeviceKeyAndPassword();

    if (this.deviceKey != null) {
      authParameters.DEVICE_KEY = this.deviceKey;
    }

    var clientMetaData = Object.keys(authDetails.getValidationData()).length !== 0 ? authDetails.getValidationData() : authDetails.getClientMetadata();
    var jsonReq = {
      AuthFlow: 'USER_PASSWORD_AUTH',
      ClientId: this.pool.getClientId(),
      AuthParameters: authParameters,
      ClientMetadata: clientMetaData
    };

    if (this.getUserContextData(this.username)) {
      jsonReq.UserContextData = this.getUserContextData(this.username);
    } // USER_PASSWORD_AUTH happens in a single round-trip: client sends userName and password,
    // Cognito UserPools verifies password and returns tokens.


    this.client.request('InitiateAuth', jsonReq, function (err, authResult) {
      if (err) {
        return callback.onFailure(err);
      }

      return _this3.authenticateUserInternal(authResult, authenticationHelper, callback);
    });
  }
  /**
   * PRIVATE ONLY: This is an internal only method and should not
   * be directly called by the consumers.
   * @param {object} dataAuthenticate authentication data
   * @param {object} authenticationHelper helper created
   * @param {callback} callback passed on from caller
   * @returns {void}
   */
  ;

  _proto.authenticateUserInternal = function authenticateUserInternal(dataAuthenticate, authenticationHelper, callback) {
    var _this4 = this;

    var challengeName = dataAuthenticate.ChallengeName;
    var challengeParameters = dataAuthenticate.ChallengeParameters;

    if (challengeName === 'SMS_MFA') {
      this.Session = dataAuthenticate.Session;
      return callback.mfaRequired(challengeName, challengeParameters);
    }

    if (challengeName === 'SELECT_MFA_TYPE') {
      this.Session = dataAuthenticate.Session;
      return callback.selectMFAType(challengeName, challengeParameters);
    }

    if (challengeName === 'MFA_SETUP') {
      this.Session = dataAuthenticate.Session;
      return callback.mfaSetup(challengeName, challengeParameters);
    }

    if (challengeName === 'SOFTWARE_TOKEN_MFA') {
      this.Session = dataAuthenticate.Session;
      return callback.totpRequired(challengeName, challengeParameters);
    }

    if (challengeName === 'CUSTOM_CHALLENGE') {
      this.Session = dataAuthenticate.Session;
      return callback.customChallenge(challengeParameters);
    }

    if (challengeName === 'NEW_PASSWORD_REQUIRED') {
      this.Session = dataAuthenticate.Session;
      var userAttributes = null;
      var rawRequiredAttributes = null;
      var requiredAttributes = [];
      var userAttributesPrefix = authenticationHelper.getNewPasswordRequiredChallengeUserAttributePrefix();

      if (challengeParameters) {
        userAttributes = JSON.parse(dataAuthenticate.ChallengeParameters.userAttributes);
        rawRequiredAttributes = JSON.parse(dataAuthenticate.ChallengeParameters.requiredAttributes);
      }

      if (rawRequiredAttributes) {
        for (var i = 0; i < rawRequiredAttributes.length; i++) {
          requiredAttributes[i] = rawRequiredAttributes[i].substr(userAttributesPrefix.length);
        }
      }

      return callback.newPasswordRequired(userAttributes, requiredAttributes);
    }

    if (challengeName === 'DEVICE_SRP_AUTH') {
      this.Session = dataAuthenticate.Session;
      this.getDeviceResponse(callback);
      return undefined;
    }

    this.signInUserSession = this.getCognitoUserSession(dataAuthenticate.AuthenticationResult);
    this.challengeName = challengeName;
    this.cacheTokens();
    var newDeviceMetadata = dataAuthenticate.AuthenticationResult.NewDeviceMetadata;

    if (newDeviceMetadata == null) {
      return callback.onSuccess(this.signInUserSession);
    }

    authenticationHelper.generateHashDevice(dataAuthenticate.AuthenticationResult.NewDeviceMetadata.DeviceGroupKey, dataAuthenticate.AuthenticationResult.NewDeviceMetadata.DeviceKey, function (errGenHash) {
      if (errGenHash) {
        return callback.onFailure(errGenHash);
      }

      var deviceSecretVerifierConfig = {
        Salt: Buffer.from(authenticationHelper.getSaltDevices(), 'hex').toString('base64'),
        PasswordVerifier: Buffer.from(authenticationHelper.getVerifierDevices(), 'hex').toString('base64')
      };
      _this4.verifierDevices = deviceSecretVerifierConfig.PasswordVerifier;
      _this4.deviceGroupKey = newDeviceMetadata.DeviceGroupKey;
      _this4.randomPassword = authenticationHelper.getRandomPassword();

      _this4.client.request('ConfirmDevice', {
        DeviceKey: newDeviceMetadata.DeviceKey,
        AccessToken: _this4.signInUserSession.getAccessToken().getJwtToken(),
        DeviceSecretVerifierConfig: deviceSecretVerifierConfig,
        DeviceName: userAgent
      }, function (errConfirm, dataConfirm) {
        if (errConfirm) {
          return callback.onFailure(errConfirm);
        }

        _this4.deviceKey = dataAuthenticate.AuthenticationResult.NewDeviceMetadata.DeviceKey;

        _this4.cacheDeviceKeyAndPassword();

        if (dataConfirm.UserConfirmationNecessary === true) {
          return callback.onSuccess(_this4.signInUserSession, dataConfirm.UserConfirmationNecessary);
        }

        return callback.onSuccess(_this4.signInUserSession);
      });

      return undefined;
    });
    return undefined;
  }
  /**
   * This method is user to complete the NEW_PASSWORD_REQUIRED challenge.
   * Pass the new password with any new user attributes to be updated.
   * User attribute keys must be of format userAttributes.<attribute_name>.
   * @param {string} newPassword new password for this user
   * @param {object} requiredAttributeData map with values for all required attributes
   * @param {object} callback Result callback map.
   * @param {onFailure} callback.onFailure Called on any error.
   * @param {mfaRequired} callback.mfaRequired MFA code required to continue.
   * @param {customChallenge} callback.customChallenge Custom challenge
   *         response required to continue.
   * @param {authSuccess} callback.onSuccess Called on success with the new session.
   * @param {ClientMetadata} clientMetadata object which is passed from client to Cognito Lambda trigger
   * @returns {void}
   */
  ;

  _proto.completeNewPasswordChallenge = function completeNewPasswordChallenge(newPassword, requiredAttributeData, callback, clientMetadata) {
    var _this5 = this;

    if (!newPassword) {
      return callback.onFailure(new Error('New password is required.'));
    }

    var authenticationHelper = new AuthenticationHelper(this.pool.getUserPoolId().split('_')[1]);
    var userAttributesPrefix = authenticationHelper.getNewPasswordRequiredChallengeUserAttributePrefix();
    var finalUserAttributes = {};

    if (requiredAttributeData) {
      Object.keys(requiredAttributeData).forEach(function (key) {
        finalUserAttributes[userAttributesPrefix + key] = requiredAttributeData[key];
      });
    }

    finalUserAttributes.NEW_PASSWORD = newPassword;
    finalUserAttributes.USERNAME = this.username;
    var jsonReq = {
      ChallengeName: 'NEW_PASSWORD_REQUIRED',
      ClientId: this.pool.getClientId(),
      ChallengeResponses: finalUserAttributes,
      Session: this.Session,
      ClientMetadata: clientMetadata
    };

    if (this.getUserContextData()) {
      jsonReq.UserContextData = this.getUserContextData();
    }

    this.client.request('RespondToAuthChallenge', jsonReq, function (errAuthenticate, dataAuthenticate) {
      if (errAuthenticate) {
        return callback.onFailure(errAuthenticate);
      }

      return _this5.authenticateUserInternal(dataAuthenticate, authenticationHelper, callback);
    });
    return undefined;
  }
  /**
   * This is used to get a session using device authentication. It is called at the end of user
   * authentication
   *
   * @param {object} callback Result callback map.
   * @param {onFailure} callback.onFailure Called on any error.
   * @param {authSuccess} callback.onSuccess Called on success with the new session.
   * @param {ClientMetadata} clientMetadata object which is passed from client to Cognito Lambda trigger
   * @returns {void}
   * @private
   */
  ;

  _proto.getDeviceResponse = function getDeviceResponse(callback, clientMetadata) {
    var _this6 = this;

    var authenticationHelper = new AuthenticationHelper(this.deviceGroupKey);
    var dateHelper = new DateHelper();
    var authParameters = {};
    authParameters.USERNAME = this.username;
    authParameters.DEVICE_KEY = this.deviceKey;
    authenticationHelper.getLargeAValue(function (errAValue, aValue) {
      // getLargeAValue callback start
      if (errAValue) {
        callback.onFailure(errAValue);
      }

      authParameters.SRP_A = aValue.toString(16);
      var jsonReq = {
        ChallengeName: 'DEVICE_SRP_AUTH',
        ClientId: _this6.pool.getClientId(),
        ChallengeResponses: authParameters,
        ClientMetadata: clientMetadata,
        Session: _this6.Session
      };

      if (_this6.getUserContextData()) {
        jsonReq.UserContextData = _this6.getUserContextData();
      }

      _this6.client.request('RespondToAuthChallenge', jsonReq, function (err, data) {
        if (err) {
          return callback.onFailure(err);
        }

        var challengeParameters = data.ChallengeParameters;
        var serverBValue = new BigInteger(challengeParameters.SRP_B, 16);
        var salt = new BigInteger(challengeParameters.SALT, 16);
        authenticationHelper.getPasswordAuthenticationKey(_this6.deviceKey, _this6.randomPassword, serverBValue, salt, function (errHkdf, hkdf) {
          // getPasswordAuthenticationKey callback start
          if (errHkdf) {
            return callback.onFailure(errHkdf);
          }

          var dateNow = dateHelper.getNowString();
          var message = core.lib.WordArray.create(Buffer.concat([Buffer.from(_this6.deviceGroupKey, 'utf8'), Buffer.from(_this6.deviceKey, 'utf8'), Buffer.from(challengeParameters.SECRET_BLOCK, 'base64'), Buffer.from(dateNow, 'utf8')]));
          var key = core.lib.WordArray.create(hkdf);
          var signatureString = encBase64.stringify(hmacSha256(message, key));
          var challengeResponses = {};
          challengeResponses.USERNAME = _this6.username;
          challengeResponses.PASSWORD_CLAIM_SECRET_BLOCK = challengeParameters.SECRET_BLOCK;
          challengeResponses.TIMESTAMP = dateNow;
          challengeResponses.PASSWORD_CLAIM_SIGNATURE = signatureString;
          challengeResponses.DEVICE_KEY = _this6.deviceKey;
          var jsonReqResp = {
            ChallengeName: 'DEVICE_PASSWORD_VERIFIER',
            ClientId: _this6.pool.getClientId(),
            ChallengeResponses: challengeResponses,
            Session: data.Session
          };

          if (_this6.getUserContextData()) {
            jsonReqResp.UserContextData = _this6.getUserContextData();
          }

          _this6.client.request('RespondToAuthChallenge', jsonReqResp, function (errAuthenticate, dataAuthenticate) {
            if (errAuthenticate) {
              return callback.onFailure(errAuthenticate);
            }

            _this6.signInUserSession = _this6.getCognitoUserSession(dataAuthenticate.AuthenticationResult);

            _this6.cacheTokens();

            return callback.onSuccess(_this6.signInUserSession);
          });

          return undefined; // getPasswordAuthenticationKey callback end
        });
        return undefined;
      }); // getLargeAValue callback end

    });
  }
  /**
   * This is used for a certain user to confirm the registration by using a confirmation code
   * @param {string} confirmationCode Code entered by user.
   * @param {bool} forceAliasCreation Allow migrating from an existing email / phone number.
   * @param {nodeCallback<string>} callback Called on success or error.
   * @param {ClientMetadata} clientMetadata object which is passed from client to Cognito Lambda trigger
   * @returns {void}
   */
  ;

  _proto.confirmRegistration = function confirmRegistration(confirmationCode, forceAliasCreation, callback, clientMetadata) {
    var jsonReq = {
      ClientId: this.pool.getClientId(),
      ConfirmationCode: confirmationCode,
      Username: this.username,
      ForceAliasCreation: forceAliasCreation,
      ClientMetadata: clientMetadata
    };

    if (this.getUserContextData()) {
      jsonReq.UserContextData = this.getUserContextData();
    }

    this.client.request('ConfirmSignUp', jsonReq, function (err) {
      if (err) {
        return callback(err, null);
      }

      return callback(null, 'SUCCESS');
    });
  }
  /**
   * This is used by the user once he has the responses to a custom challenge
   * @param {string} answerChallenge The custom challenge answer.
   * @param {object} callback Result callback map.
   * @param {onFailure} callback.onFailure Called on any error.
   * @param {customChallenge} callback.customChallenge
   *    Custom challenge response required to continue.
   * @param {authSuccess} callback.onSuccess Called on success with the new session.
   * @param {ClientMetadata} clientMetadata object which is passed from client to Cognito Lambda trigger
   * @returns {void}
   */
  ;

  _proto.sendCustomChallengeAnswer = function sendCustomChallengeAnswer(answerChallenge, callback, clientMetadata) {
    var _this7 = this;

    var challengeResponses = {};
    challengeResponses.USERNAME = this.username;
    challengeResponses.ANSWER = answerChallenge;
    var authenticationHelper = new AuthenticationHelper(this.pool.getUserPoolId().split('_')[1]);
    this.getCachedDeviceKeyAndPassword();

    if (this.deviceKey != null) {
      challengeResponses.DEVICE_KEY = this.deviceKey;
    }

    var jsonReq = {
      ChallengeName: 'CUSTOM_CHALLENGE',
      ChallengeResponses: challengeResponses,
      ClientId: this.pool.getClientId(),
      Session: this.Session,
      ClientMetadata: clientMetadata
    };

    if (this.getUserContextData()) {
      jsonReq.UserContextData = this.getUserContextData();
    }

    this.client.request('RespondToAuthChallenge', jsonReq, function (err, data) {
      if (err) {
        return callback.onFailure(err);
      }

      return _this7.authenticateUserInternal(data, authenticationHelper, callback);
    });
  }
  /**
   * This is used by the user once he has an MFA code
   * @param {string} confirmationCode The MFA code entered by the user.
   * @param {object} callback Result callback map.
   * @param {string} mfaType The mfa we are replying to.
   * @param {onFailure} callback.onFailure Called on any error.
   * @param {authSuccess} callback.onSuccess Called on success with the new session.
   * @param {ClientMetadata} clientMetadata object which is passed from client to Cognito Lambda trigger
   * @returns {void}
   */
  ;

  _proto.sendMFACode = function sendMFACode(confirmationCode, callback, mfaType, clientMetadata) {
    var _this8 = this;

    var challengeResponses = {};
    challengeResponses.USERNAME = this.username;
    challengeResponses.SMS_MFA_CODE = confirmationCode;
    var mfaTypeSelection = mfaType || 'SMS_MFA';

    if (mfaTypeSelection === 'SOFTWARE_TOKEN_MFA') {
      challengeResponses.SOFTWARE_TOKEN_MFA_CODE = confirmationCode;
    }

    if (this.deviceKey != null) {
      challengeResponses.DEVICE_KEY = this.deviceKey;
    }

    var jsonReq = {
      ChallengeName: mfaTypeSelection,
      ChallengeResponses: challengeResponses,
      ClientId: this.pool.getClientId(),
      Session: this.Session,
      ClientMetadata: clientMetadata
    };

    if (this.getUserContextData()) {
      jsonReq.UserContextData = this.getUserContextData();
    }

    this.client.request('RespondToAuthChallenge', jsonReq, function (err, dataAuthenticate) {
      if (err) {
        return callback.onFailure(err);
      }

      var challengeName = dataAuthenticate.ChallengeName;

      if (challengeName === 'DEVICE_SRP_AUTH') {
        _this8.getDeviceResponse(callback);

        return undefined;
      }

      _this8.signInUserSession = _this8.getCognitoUserSession(dataAuthenticate.AuthenticationResult);

      _this8.cacheTokens();

      if (dataAuthenticate.AuthenticationResult.NewDeviceMetadata == null) {
        return callback.onSuccess(_this8.signInUserSession);
      }

      var authenticationHelper = new AuthenticationHelper(_this8.pool.getUserPoolId().split('_')[1]);
      authenticationHelper.generateHashDevice(dataAuthenticate.AuthenticationResult.NewDeviceMetadata.DeviceGroupKey, dataAuthenticate.AuthenticationResult.NewDeviceMetadata.DeviceKey, function (errGenHash) {
        if (errGenHash) {
          return callback.onFailure(errGenHash);
        }

        var deviceSecretVerifierConfig = {
          Salt: Buffer.from(authenticationHelper.getSaltDevices(), 'hex').toString('base64'),
          PasswordVerifier: Buffer.from(authenticationHelper.getVerifierDevices(), 'hex').toString('base64')
        };
        _this8.verifierDevices = deviceSecretVerifierConfig.PasswordVerifier;
        _this8.deviceGroupKey = dataAuthenticate.AuthenticationResult.NewDeviceMetadata.DeviceGroupKey;
        _this8.randomPassword = authenticationHelper.getRandomPassword();

        _this8.client.request('ConfirmDevice', {
          DeviceKey: dataAuthenticate.AuthenticationResult.NewDeviceMetadata.DeviceKey,
          AccessToken: _this8.signInUserSession.getAccessToken().getJwtToken(),
          DeviceSecretVerifierConfig: deviceSecretVerifierConfig,
          DeviceName: userAgent
        }, function (errConfirm, dataConfirm) {
          if (errConfirm) {
            return callback.onFailure(errConfirm);
          }

          _this8.deviceKey = dataAuthenticate.AuthenticationResult.NewDeviceMetadata.DeviceKey;

          _this8.cacheDeviceKeyAndPassword();

          if (dataConfirm.UserConfirmationNecessary === true) {
            return callback.onSuccess(_this8.signInUserSession, dataConfirm.UserConfirmationNecessary);
          }

          return callback.onSuccess(_this8.signInUserSession);
        });

        return undefined;
      });
      return undefined;
    });
  }
  /**
   * This is used by an authenticated user to change the current password
   * @param {string} oldUserPassword The current password.
   * @param {string} newUserPassword The requested new password.
   * @param {nodeCallback<string>} callback Called on success or error.
   * @param {ClientMetadata} clientMetadata object which is passed from client to Cognito Lambda trigger
   * @returns {void}
   */
  ;

  _proto.changePassword = function changePassword(oldUserPassword, newUserPassword, callback, clientMetadata) {
    if (!(this.signInUserSession != null && this.signInUserSession.isValid())) {
      return callback(new Error('User is not authenticated'), null);
    }

    this.client.request('ChangePassword', {
      PreviousPassword: oldUserPassword,
      ProposedPassword: newUserPassword,
      AccessToken: this.signInUserSession.getAccessToken().getJwtToken(),
      ClientMetadata: clientMetadata
    }, function (err) {
      if (err) {
        return callback(err, null);
      }

      return callback(null, 'SUCCESS');
    });
    return undefined;
  }
  /**
   * This is used by an authenticated user to enable MFA for itself
   * @deprecated
   * @param {nodeCallback<string>} callback Called on success or error.
   * @returns {void}
   */
  ;

  _proto.enableMFA = function enableMFA(callback) {
    if (this.signInUserSession == null || !this.signInUserSession.isValid()) {
      return callback(new Error('User is not authenticated'), null);
    }

    var mfaOptions = [];
    var mfaEnabled = {
      DeliveryMedium: 'SMS',
      AttributeName: 'phone_number'
    };
    mfaOptions.push(mfaEnabled);
    this.client.request('SetUserSettings', {
      MFAOptions: mfaOptions,
      AccessToken: this.signInUserSession.getAccessToken().getJwtToken()
    }, function (err) {
      if (err) {
        return callback(err, null);
      }

      return callback(null, 'SUCCESS');
    });
    return undefined;
  }
  /**
   * This is used by an authenticated user to enable MFA for itself
   * @param {IMfaSettings} smsMfaSettings the sms mfa settings
   * @param {IMFASettings} softwareTokenMfaSettings the software token mfa settings
   * @param {nodeCallback<string>} callback Called on success or error.
   * @returns {void}
   */
  ;

  _proto.setUserMfaPreference = function setUserMfaPreference(smsMfaSettings, softwareTokenMfaSettings, callback) {
    if (this.signInUserSession == null || !this.signInUserSession.isValid()) {
      return callback(new Error('User is not authenticated'), null);
    }

    this.client.request('SetUserMFAPreference', {
      SMSMfaSettings: smsMfaSettings,
      SoftwareTokenMfaSettings: softwareTokenMfaSettings,
      AccessToken: this.signInUserSession.getAccessToken().getJwtToken()
    }, function (err) {
      if (err) {
        return callback(err, null);
      }

      return callback(null, 'SUCCESS');
    });
    return undefined;
  }
  /**
   * This is used by an authenticated user to disable MFA for itself
   * @deprecated
   * @param {nodeCallback<string>} callback Called on success or error.
   * @returns {void}
   */
  ;

  _proto.disableMFA = function disableMFA(callback) {
    if (this.signInUserSession == null || !this.signInUserSession.isValid()) {
      return callback(new Error('User is not authenticated'), null);
    }

    var mfaOptions = [];
    this.client.request('SetUserSettings', {
      MFAOptions: mfaOptions,
      AccessToken: this.signInUserSession.getAccessToken().getJwtToken()
    }, function (err) {
      if (err) {
        return callback(err, null);
      }

      return callback(null, 'SUCCESS');
    });
    return undefined;
  }
  /**
   * This is used by an authenticated user to delete itself
   * @param {nodeCallback<string>} callback Called on success or error.
   * @param {ClientMetadata} clientMetadata object which is passed from client to Cognito Lambda trigger
   * @returns {void}
   */
  ;

  _proto.deleteUser = function deleteUser(callback, clientMetadata) {
    var _this9 = this;

    if (this.signInUserSession == null || !this.signInUserSession.isValid()) {
      return callback(new Error('User is not authenticated'), null);
    }

    this.client.request('DeleteUser', {
      AccessToken: this.signInUserSession.getAccessToken().getJwtToken(),
      ClientMetadata: clientMetadata
    }, function (err) {
      if (err) {
        return callback(err, null);
      }

      _this9.clearCachedUser();

      return callback(null, 'SUCCESS');
    });
    return undefined;
  }
  /**
   * @typedef {CognitoUserAttribute | { Name:string, Value:string }} AttributeArg
   */

  /**
   * This is used by an authenticated user to change a list of attributes
   * @param {AttributeArg[]} attributes A list of the new user attributes.
   * @param {nodeCallback<string>} callback Called on success or error.
   * @param {ClientMetadata} clientMetadata object which is passed from client to Cognito Lambda trigger
   * @returns {void}
   */
  ;

  _proto.updateAttributes = function updateAttributes(attributes, callback, clientMetadata) {
    var _this10 = this;

    if (this.signInUserSession == null || !this.signInUserSession.isValid()) {
      return callback(new Error('User is not authenticated'), null);
    }

    this.client.request('UpdateUserAttributes', {
      AccessToken: this.signInUserSession.getAccessToken().getJwtToken(),
      UserAttributes: attributes,
      ClientMetadata: clientMetadata
    }, function (err) {
      if (err) {
        return callback(err, null);
      } // update cached user


      return _this10.getUserData(function () {
        return callback(null, 'SUCCESS');
      }, {
        bypassCache: true
      });
    });
    return undefined;
  }
  /**
   * This is used by an authenticated user to get a list of attributes
   * @param {nodeCallback<CognitoUserAttribute[]>} callback Called on success or error.
   * @returns {void}
   */
  ;

  _proto.getUserAttributes = function getUserAttributes(callback) {
    if (!(this.signInUserSession != null && this.signInUserSession.isValid())) {
      return callback(new Error('User is not authenticated'), null);
    }

    this.client.request('GetUser', {
      AccessToken: this.signInUserSession.getAccessToken().getJwtToken()
    }, function (err, userData) {
      if (err) {
        return callback(err, null);
      }

      var attributeList = [];

      for (var i = 0; i < userData.UserAttributes.length; i++) {
        var attribute = {
          Name: userData.UserAttributes[i].Name,
          Value: userData.UserAttributes[i].Value
        };
        var userAttribute = new CognitoUserAttribute(attribute);
        attributeList.push(userAttribute);
      }

      return callback(null, attributeList);
    });
    return undefined;
  }
  /**
   * This was previously used by an authenticated user to get MFAOptions,
   * but no longer returns a meaningful response. Refer to the documentation for
   * how to setup and use MFA: https://docs.amplify.aws/lib/auth/mfa/q/platform/js
   * @deprecated
   * @param {nodeCallback<MFAOptions>} callback Called on success or error.
   * @returns {void}
   */
  ;

  _proto.getMFAOptions = function getMFAOptions(callback) {
    if (!(this.signInUserSession != null && this.signInUserSession.isValid())) {
      return callback(new Error('User is not authenticated'), null);
    }

    this.client.request('GetUser', {
      AccessToken: this.signInUserSession.getAccessToken().getJwtToken()
    }, function (err, userData) {
      if (err) {
        return callback(err, null);
      }

      return callback(null, userData.MFAOptions);
    });
    return undefined;
  }
  /**
   * PRIVATE ONLY: This is an internal only method and should not
   * be directly called by the consumers.
   */
  ;

  _proto.createGetUserRequest = function createGetUserRequest() {
    return this.client.promisifyRequest('GetUser', {
      AccessToken: this.signInUserSession.getAccessToken().getJwtToken()
    });
  }
  /**
   * PRIVATE ONLY: This is an internal only method and should not
   * be directly called by the consumers.
   */
  ;

  _proto.refreshSessionIfPossible = function refreshSessionIfPossible(options) {
    var _this11 = this;

    if (options === void 0) {
      options = {};
    }

    // best effort, if not possible
    return new Promise(function (resolve) {
      var refresh = _this11.signInUserSession.getRefreshToken();

      if (refresh && refresh.getToken()) {
        _this11.refreshSession(refresh, resolve, options.clientMetadata);
      } else {
        resolve();
      }
    });
  }
  /**
   * @typedef {Object} GetUserDataOptions
   * @property {boolean} bypassCache - force getting data from Cognito service
   * @property {Record<string, string>} clientMetadata - clientMetadata for getSession
   */

  /**
   * This is used by an authenticated users to get the userData
   * @param {nodeCallback<UserData>} callback Called on success or error.
   * @param {GetUserDataOptions} params
   * @returns {void}
   */
  ;

  _proto.getUserData = function getUserData(callback, params) {
    var _this12 = this;

    if (!(this.signInUserSession != null && this.signInUserSession.isValid())) {
      this.clearCachedUserData();
      return callback(new Error('User is not authenticated'), null);
    }

    var userData = this.getUserDataFromCache();

    if (!userData) {
      this.fetchUserData().then(function (data) {
        callback(null, data);
      })["catch"](callback);
      return;
    }

    if (this.isFetchUserDataAndTokenRequired(params)) {
      this.fetchUserData().then(function (data) {
        return _this12.refreshSessionIfPossible(params).then(function () {
          return data;
        });
      }).then(function (data) {
        return callback(null, data);
      })["catch"](callback);
      return;
    }

    try {
      callback(null, JSON.parse(userData));
      return;
    } catch (err) {
      this.clearCachedUserData();
      callback(err, null);
      return;
    }
  }
  /**
   *
   * PRIVATE ONLY: This is an internal only method and should not
   * be directly called by the consumers.
   */
  ;

  _proto.getUserDataFromCache = function getUserDataFromCache() {
    var userData = this.storage.getItem(this.userDataKey);
    return userData;
  }
  /**
   *
   * PRIVATE ONLY: This is an internal only method and should not
   * be directly called by the consumers.
   */
  ;

  _proto.isFetchUserDataAndTokenRequired = function isFetchUserDataAndTokenRequired(params) {
    var _ref = params || {},
        _ref$bypassCache = _ref.bypassCache,
        bypassCache = _ref$bypassCache === void 0 ? false : _ref$bypassCache;

    return bypassCache;
  }
  /**
   *
   * PRIVATE ONLY: This is an internal only method and should not
   * be directly called by the consumers.
   */
  ;

  _proto.fetchUserData = function fetchUserData() {
    var _this13 = this;

    return this.createGetUserRequest().then(function (data) {
      _this13.cacheUserData(data);

      return data;
    });
  }
  /**
   * This is used by an authenticated user to delete a list of attributes
   * @param {string[]} attributeList Names of the attributes to delete.
   * @param {nodeCallback<string>} callback Called on success or error.
   * @returns {void}
   */
  ;

  _proto.deleteAttributes = function deleteAttributes(attributeList, callback) {
    var _this14 = this;

    if (!(this.signInUserSession != null && this.signInUserSession.isValid())) {
      return callback(new Error('User is not authenticated'), null);
    }

    this.client.request('DeleteUserAttributes', {
      UserAttributeNames: attributeList,
      AccessToken: this.signInUserSession.getAccessToken().getJwtToken()
    }, function (err) {
      if (err) {
        return callback(err, null);
      } // update cached user


      return _this14.getUserData(function () {
        return callback(null, 'SUCCESS');
      }, {
        bypassCache: true
      });
    });
    return undefined;
  }
  /**
   * This is used by a user to resend a confirmation code
   * @param {nodeCallback<string>} callback Called on success or error.
   * @param {ClientMetadata} clientMetadata object which is passed from client to Cognito Lambda trigger
   * @returns {void}
   */
  ;

  _proto.resendConfirmationCode = function resendConfirmationCode(callback, clientMetadata) {
    var jsonReq = {
      ClientId: this.pool.getClientId(),
      Username: this.username,
      ClientMetadata: clientMetadata
    };
    this.client.request('ResendConfirmationCode', jsonReq, function (err, result) {
      if (err) {
        return callback(err, null);
      }

      return callback(null, result);
    });
  }
  /**
   * @typedef {Object} GetSessionOptions
   * @property {Record<string, string>} clientMetadata - clientMetadata for getSession
   */

  /**
   * This is used to get a session, either from the session object
   * or from  the local storage, or by using a refresh token
   *
   * @param {nodeCallback<CognitoUserSession>} callback Called on success or error.
   * @param {GetSessionOptions} options
   * @returns {void}
   */
  ;

  _proto.getSession = function getSession(callback, options) {
    if (options === void 0) {
      options = {};
    }

    if (this.username == null) {
      return callback(new Error('Username is null. Cannot retrieve a new session'), null);
    }

    if (this.signInUserSession != null && this.signInUserSession.isValid()) {
      return callback(null, this.signInUserSession);
    }

    var keyPrefix = "CognitoIdentityServiceProvider." + this.pool.getClientId() + "." + this.username;
    var idTokenKey = keyPrefix + ".idToken";
    var accessTokenKey = keyPrefix + ".accessToken";
    var refreshTokenKey = keyPrefix + ".refreshToken";
    var clockDriftKey = keyPrefix + ".clockDrift";

    if (this.storage.getItem(idTokenKey)) {
      var idToken = new CognitoIdToken({
        IdToken: this.storage.getItem(idTokenKey)
      });
      var accessToken = new CognitoAccessToken({
        AccessToken: this.storage.getItem(accessTokenKey)
      });
      var refreshToken = new CognitoRefreshToken({
        RefreshToken: this.storage.getItem(refreshTokenKey)
      });
      var clockDrift = parseInt(this.storage.getItem(clockDriftKey), 0) || 0;
      var sessionData = {
        IdToken: idToken,
        AccessToken: accessToken,
        RefreshToken: refreshToken,
        ClockDrift: clockDrift
      };
      var cachedSession = new CognitoUserSession(sessionData);

      if (cachedSession.isValid()) {
        this.signInUserSession = cachedSession;
        return callback(null, this.signInUserSession);
      }

      if (!refreshToken.getToken()) {
        return callback(new Error('Cannot retrieve a new session. Please authenticate.'), null);
      }

      this.refreshSession(refreshToken, callback, options.clientMetadata);
    } else {
      callback(new Error('Local storage is missing an ID Token, Please authenticate'), null);
    }

    return undefined;
  }
  /**
   * This uses the refreshToken to retrieve a new session
   * @param {CognitoRefreshToken} refreshToken A previous session's refresh token.
   * @param {nodeCallback<CognitoUserSession>} callback Called on success or error.
   * @param {ClientMetadata} clientMetadata object which is passed from client to Cognito Lambda trigger
   * @returns {void}
   */
  ;

  _proto.refreshSession = function refreshSession(refreshToken, callback, clientMetadata) {
    var _this15 = this;

    var wrappedCallback = this.pool.wrapRefreshSessionCallback ? this.pool.wrapRefreshSessionCallback(callback) : callback;
    var authParameters = {};
    authParameters.REFRESH_TOKEN = refreshToken.getToken();
    var keyPrefix = "CognitoIdentityServiceProvider." + this.pool.getClientId();
    var lastUserKey = keyPrefix + ".LastAuthUser";

    if (this.storage.getItem(lastUserKey)) {
      this.username = this.storage.getItem(lastUserKey);
      var deviceKeyKey = keyPrefix + "." + this.username + ".deviceKey";
      this.deviceKey = this.storage.getItem(deviceKeyKey);
      authParameters.DEVICE_KEY = this.deviceKey;
    }

    var jsonReq = {
      ClientId: this.pool.getClientId(),
      AuthFlow: 'REFRESH_TOKEN_AUTH',
      AuthParameters: authParameters,
      ClientMetadata: clientMetadata
    };

    if (this.getUserContextData()) {
      jsonReq.UserContextData = this.getUserContextData();
    }

    this.client.request('InitiateAuth', jsonReq, function (err, authResult) {
      if (err) {
        if (err.code === 'NotAuthorizedException') {
          _this15.clearCachedUser();
        }

        return wrappedCallback(err, null);
      }

      if (authResult) {
        var authenticationResult = authResult.AuthenticationResult;

        if (!Object.prototype.hasOwnProperty.call(authenticationResult, 'RefreshToken')) {
          authenticationResult.RefreshToken = refreshToken.getToken();
        }

        _this15.signInUserSession = _this15.getCognitoUserSession(authenticationResult);

        _this15.cacheTokens();

        return wrappedCallback(null, _this15.signInUserSession);
      }

      return undefined;
    });
  }
  /**
   * This is used to save the session tokens to local storage
   * @returns {void}
   */
  ;

  _proto.cacheTokens = function cacheTokens() {
    var keyPrefix = "CognitoIdentityServiceProvider." + this.pool.getClientId();
    var idTokenKey = keyPrefix + "." + this.username + ".idToken";
    var accessTokenKey = keyPrefix + "." + this.username + ".accessToken";
    var refreshTokenKey = keyPrefix + "." + this.username + ".refreshToken";
    var clockDriftKey = keyPrefix + "." + this.username + ".clockDrift";
    var lastUserKey = keyPrefix + ".LastAuthUser";
    this.storage.setItem(idTokenKey, this.signInUserSession.getIdToken().getJwtToken());
    this.storage.setItem(accessTokenKey, this.signInUserSession.getAccessToken().getJwtToken());
    this.storage.setItem(refreshTokenKey, this.signInUserSession.getRefreshToken().getToken());
    this.storage.setItem(clockDriftKey, "" + this.signInUserSession.getClockDrift());
    this.storage.setItem(lastUserKey, this.username);
  }
  /**
   * This is to cache user data
   */
  ;

  _proto.cacheUserData = function cacheUserData(userData) {
    this.storage.setItem(this.userDataKey, JSON.stringify(userData));
  }
  /**
   * This is to remove cached user data
   */
  ;

  _proto.clearCachedUserData = function clearCachedUserData() {
    this.storage.removeItem(this.userDataKey);
  };

  _proto.clearCachedUser = function clearCachedUser() {
    this.clearCachedTokens();
    this.clearCachedUserData();
  }
  /**
   * This is used to cache the device key and device group and device password
   * @returns {void}
   */
  ;

  _proto.cacheDeviceKeyAndPassword = function cacheDeviceKeyAndPassword() {
    var keyPrefix = "CognitoIdentityServiceProvider." + this.pool.getClientId() + "." + this.username;
    var deviceKeyKey = keyPrefix + ".deviceKey";
    var randomPasswordKey = keyPrefix + ".randomPasswordKey";
    var deviceGroupKeyKey = keyPrefix + ".deviceGroupKey";
    this.storage.setItem(deviceKeyKey, this.deviceKey);
    this.storage.setItem(randomPasswordKey, this.randomPassword);
    this.storage.setItem(deviceGroupKeyKey, this.deviceGroupKey);
  }
  /**
   * This is used to get current device key and device group and device password
   * @returns {void}
   */
  ;

  _proto.getCachedDeviceKeyAndPassword = function getCachedDeviceKeyAndPassword() {
    var keyPrefix = "CognitoIdentityServiceProvider." + this.pool.getClientId() + "." + this.username;
    var deviceKeyKey = keyPrefix + ".deviceKey";
    var randomPasswordKey = keyPrefix + ".randomPasswordKey";
    var deviceGroupKeyKey = keyPrefix + ".deviceGroupKey";

    if (this.storage.getItem(deviceKeyKey)) {
      this.deviceKey = this.storage.getItem(deviceKeyKey);
      this.randomPassword = this.storage.getItem(randomPasswordKey);
      this.deviceGroupKey = this.storage.getItem(deviceGroupKeyKey);
    }
  }
  /**
   * This is used to clear the device key info from local storage
   * @returns {void}
   */
  ;

  _proto.clearCachedDeviceKeyAndPassword = function clearCachedDeviceKeyAndPassword() {
    var keyPrefix = "CognitoIdentityServiceProvider." + this.pool.getClientId() + "." + this.username;
    var deviceKeyKey = keyPrefix + ".deviceKey";
    var randomPasswordKey = keyPrefix + ".randomPasswordKey";
    var deviceGroupKeyKey = keyPrefix + ".deviceGroupKey";
    this.storage.removeItem(deviceKeyKey);
    this.storage.removeItem(randomPasswordKey);
    this.storage.removeItem(deviceGroupKeyKey);
  }
  /**
   * This is used to clear the session tokens from local storage
   * @returns {void}
   */
  ;

  _proto.clearCachedTokens = function clearCachedTokens() {
    var keyPrefix = "CognitoIdentityServiceProvider." + this.pool.getClientId();
    var idTokenKey = keyPrefix + "." + this.username + ".idToken";
    var accessTokenKey = keyPrefix + "." + this.username + ".accessToken";
    var refreshTokenKey = keyPrefix + "." + this.username + ".refreshToken";
    var lastUserKey = keyPrefix + ".LastAuthUser";
    var clockDriftKey = keyPrefix + "." + this.username + ".clockDrift";
    this.storage.removeItem(idTokenKey);
    this.storage.removeItem(accessTokenKey);
    this.storage.removeItem(refreshTokenKey);
    this.storage.removeItem(lastUserKey);
    this.storage.removeItem(clockDriftKey);
  }
  /**
   * This is used to build a user session from tokens retrieved in the authentication result
   * @param {object} authResult Successful auth response from server.
   * @returns {CognitoUserSession} The new user session.
   * @private
   */
  ;

  _proto.getCognitoUserSession = function getCognitoUserSession(authResult) {
    var idToken = new CognitoIdToken(authResult);
    var accessToken = new CognitoAccessToken(authResult);
    var refreshToken = new CognitoRefreshToken(authResult);
    var sessionData = {
      IdToken: idToken,
      AccessToken: accessToken,
      RefreshToken: refreshToken
    };
    return new CognitoUserSession(sessionData);
  }
  /**
   * This is used to initiate a forgot password request
   * @param {object} callback Result callback map.
   * @param {onFailure} callback.onFailure Called on any error.
   * @param {inputVerificationCode?} callback.inputVerificationCode
   *    Optional callback raised instead of onSuccess with response data.
   * @param {onSuccess} callback.onSuccess Called on success.
   * @param {ClientMetadata} clientMetadata object which is passed from client to Cognito Lambda trigger
   * @returns {void}
   */
  ;

  _proto.forgotPassword = function forgotPassword(callback, clientMetadata) {
    var jsonReq = {
      ClientId: this.pool.getClientId(),
      Username: this.username,
      ClientMetadata: clientMetadata
    };

    if (this.getUserContextData()) {
      jsonReq.UserContextData = this.getUserContextData();
    }

    this.client.request('ForgotPassword', jsonReq, function (err, data) {
      if (err) {
        return callback.onFailure(err);
      }

      if (typeof callback.inputVerificationCode === 'function') {
        return callback.inputVerificationCode(data);
      }

      return callback.onSuccess(data);
    });
  }
  /**
   * This is used to confirm a new password using a confirmationCode
   * @param {string} confirmationCode Code entered by user.
   * @param {string} newPassword Confirm new password.
   * @param {object} callback Result callback map.
   * @param {onFailure} callback.onFailure Called on any error.
   * @param {onSuccess<void>} callback.onSuccess Called on success.
   * @param {ClientMetadata} clientMetadata object which is passed from client to Cognito Lambda trigger
   * @returns {void}
   */
  ;

  _proto.confirmPassword = function confirmPassword(confirmationCode, newPassword, callback, clientMetadata) {
    var jsonReq = {
      ClientId: this.pool.getClientId(),
      Username: this.username,
      ConfirmationCode: confirmationCode,
      Password: newPassword,
      ClientMetadata: clientMetadata
    };

    if (this.getUserContextData()) {
      jsonReq.UserContextData = this.getUserContextData();
    }

    this.client.request('ConfirmForgotPassword', jsonReq, function (err) {
      if (err) {
        return callback.onFailure(err);
      }

      return callback.onSuccess('SUCCESS');
    });
  }
  /**
   * This is used to initiate an attribute confirmation request
   * @param {string} attributeName User attribute that needs confirmation.
   * @param {object} callback Result callback map.
   * @param {onFailure} callback.onFailure Called on any error.
   * @param {inputVerificationCode} callback.inputVerificationCode Called on success.
   * @param {ClientMetadata} clientMetadata object which is passed from client to Cognito Lambda trigger
   * @returns {void}
   */
  ;

  _proto.getAttributeVerificationCode = function getAttributeVerificationCode(attributeName, callback, clientMetadata) {
    if (this.signInUserSession == null || !this.signInUserSession.isValid()) {
      return callback.onFailure(new Error('User is not authenticated'));
    }

    this.client.request('GetUserAttributeVerificationCode', {
      AttributeName: attributeName,
      AccessToken: this.signInUserSession.getAccessToken().getJwtToken(),
      ClientMetadata: clientMetadata
    }, function (err, data) {
      if (err) {
        return callback.onFailure(err);
      }

      if (typeof callback.inputVerificationCode === 'function') {
        return callback.inputVerificationCode(data);
      }

      return callback.onSuccess('SUCCESS');
    });
    return undefined;
  }
  /**
   * This is used to confirm an attribute using a confirmation code
   * @param {string} attributeName Attribute being confirmed.
   * @param {string} confirmationCode Code entered by user.
   * @param {object} callback Result callback map.
   * @param {onFailure} callback.onFailure Called on any error.
   * @param {onSuccess<string>} callback.onSuccess Called on success.
   * @returns {void}
   */
  ;

  _proto.verifyAttribute = function verifyAttribute(attributeName, confirmationCode, callback) {
    if (this.signInUserSession == null || !this.signInUserSession.isValid()) {
      return callback.onFailure(new Error('User is not authenticated'));
    }

    this.client.request('VerifyUserAttribute', {
      AttributeName: attributeName,
      Code: confirmationCode,
      AccessToken: this.signInUserSession.getAccessToken().getJwtToken()
    }, function (err) {
      if (err) {
        return callback.onFailure(err);
      }

      return callback.onSuccess('SUCCESS');
    });
    return undefined;
  }
  /**
   * This is used to get the device information using the current device key
   * @param {object} callback Result callback map.
   * @param {onFailure} callback.onFailure Called on any error.
   * @param {onSuccess<*>} callback.onSuccess Called on success with device data.
   * @returns {void}
   */
  ;

  _proto.getDevice = function getDevice(callback) {
    if (this.signInUserSession == null || !this.signInUserSession.isValid()) {
      return callback.onFailure(new Error('User is not authenticated'));
    }

    this.client.request('GetDevice', {
      AccessToken: this.signInUserSession.getAccessToken().getJwtToken(),
      DeviceKey: this.deviceKey
    }, function (err, data) {
      if (err) {
        return callback.onFailure(err);
      }

      return callback.onSuccess(data);
    });
    return undefined;
  }
  /**
   * This is used to forget a specific device
   * @param {string} deviceKey Device key.
   * @param {object} callback Result callback map.
   * @param {onFailure} callback.onFailure Called on any error.
   * @param {onSuccess<string>} callback.onSuccess Called on success.
   * @returns {void}
   */
  ;

  _proto.forgetSpecificDevice = function forgetSpecificDevice(deviceKey, callback) {
    if (this.signInUserSession == null || !this.signInUserSession.isValid()) {
      return callback.onFailure(new Error('User is not authenticated'));
    }

    this.client.request('ForgetDevice', {
      AccessToken: this.signInUserSession.getAccessToken().getJwtToken(),
      DeviceKey: deviceKey
    }, function (err) {
      if (err) {
        return callback.onFailure(err);
      }

      return callback.onSuccess('SUCCESS');
    });
    return undefined;
  }
  /**
   * This is used to forget the current device
   * @param {object} callback Result callback map.
   * @param {onFailure} callback.onFailure Called on any error.
   * @param {onSuccess<string>} callback.onSuccess Called on success.
   * @returns {void}
   */
  ;

  _proto.forgetDevice = function forgetDevice(callback) {
    var _this16 = this;

    this.forgetSpecificDevice(this.deviceKey, {
      onFailure: callback.onFailure,
      onSuccess: function onSuccess(result) {
        _this16.deviceKey = null;
        _this16.deviceGroupKey = null;
        _this16.randomPassword = null;

        _this16.clearCachedDeviceKeyAndPassword();

        return callback.onSuccess(result);
      }
    });
  }
  /**
   * This is used to set the device status as remembered
   * @param {object} callback Result callback map.
   * @param {onFailure} callback.onFailure Called on any error.
   * @param {onSuccess<string>} callback.onSuccess Called on success.
   * @returns {void}
   */
  ;

  _proto.setDeviceStatusRemembered = function setDeviceStatusRemembered(callback) {
    if (this.signInUserSession == null || !this.signInUserSession.isValid()) {
      return callback.onFailure(new Error('User is not authenticated'));
    }

    this.client.request('UpdateDeviceStatus', {
      AccessToken: this.signInUserSession.getAccessToken().getJwtToken(),
      DeviceKey: this.deviceKey,
      DeviceRememberedStatus: 'remembered'
    }, function (err) {
      if (err) {
        return callback.onFailure(err);
      }

      return callback.onSuccess('SUCCESS');
    });
    return undefined;
  }
  /**
   * This is used to set the device status as not remembered
   * @param {object} callback Result callback map.
   * @param {onFailure} callback.onFailure Called on any error.
   * @param {onSuccess<string>} callback.onSuccess Called on success.
   * @returns {void}
   */
  ;

  _proto.setDeviceStatusNotRemembered = function setDeviceStatusNotRemembered(callback) {
    if (this.signInUserSession == null || !this.signInUserSession.isValid()) {
      return callback.onFailure(new Error('User is not authenticated'));
    }

    this.client.request('UpdateDeviceStatus', {
      AccessToken: this.signInUserSession.getAccessToken().getJwtToken(),
      DeviceKey: this.deviceKey,
      DeviceRememberedStatus: 'not_remembered'
    }, function (err) {
      if (err) {
        return callback.onFailure(err);
      }

      return callback.onSuccess('SUCCESS');
    });
    return undefined;
  }
  /**
   * This is used to list all devices for a user
   *
   * @param {int} limit the number of devices returned in a call
   * @param {string | null} paginationToken the pagination token in case any was returned before
   * @param {object} callback Result callback map.
   * @param {onFailure} callback.onFailure Called on any error.
   * @param {onSuccess<*>} callback.onSuccess Called on success with device list.
   * @returns {void}
   */
  ;

  _proto.listDevices = function listDevices(limit, paginationToken, callback) {
    if (this.signInUserSession == null || !this.signInUserSession.isValid()) {
      return callback.onFailure(new Error('User is not authenticated'));
    }

    var requestParams = {
      AccessToken: this.signInUserSession.getAccessToken().getJwtToken(),
      Limit: limit
    };

    if (paginationToken) {
      requestParams.PaginationToken = paginationToken;
    }

    this.client.request('ListDevices', requestParams, function (err, data) {
      if (err) {
        return callback.onFailure(err);
      }

      return callback.onSuccess(data);
    });
    return undefined;
  }
  /**
   * This is used to globally revoke all tokens issued to a user
   * @param {object} callback Result callback map.
   * @param {onFailure} callback.onFailure Called on any error.
   * @param {onSuccess<string>} callback.onSuccess Called on success.
   * @returns {void}
   */
  ;

  _proto.globalSignOut = function globalSignOut(callback) {
    var _this17 = this;

    if (this.signInUserSession == null || !this.signInUserSession.isValid()) {
      return callback.onFailure(new Error('User is not authenticated'));
    }

    this.client.request('GlobalSignOut', {
      AccessToken: this.signInUserSession.getAccessToken().getJwtToken()
    }, function (err) {
      if (err) {
        return callback.onFailure(err);
      }

      _this17.clearCachedUser();

      return callback.onSuccess('SUCCESS');
    });
    return undefined;
  }
  /**
   * This is used for the user to signOut of the application and clear the cached tokens.
   * @returns {void}
   */
  ;

  _proto.signOut = function signOut(revokeTokenCallback) {
    var _this18 = this;

    // If tokens won't be revoked, we just clean the client data.
    if (!revokeTokenCallback || typeof revokeTokenCallback !== 'function') {
      this.cleanClientData();
      return;
    }

    this.getSession(function (error, _session) {
      if (error) {
        return revokeTokenCallback(error);
      }

      _this18.revokeTokens(function (err) {
        _this18.cleanClientData();

        revokeTokenCallback(err);
      });
    });
  };

  _proto.revokeTokens = function revokeTokens(revokeTokenCallback) {
    if (revokeTokenCallback === void 0) {
      revokeTokenCallback = function revokeTokenCallback() {};
    }

    if (typeof revokeTokenCallback !== 'function') {
      throw new Error('Invalid revokeTokenCallback. It should be a function.');
    }

    if (!this.signInUserSession) {
      var error = new Error('User is not authenticated');
      return revokeTokenCallback(error);
    }

    if (!this.signInUserSession.getAccessToken()) {
      var _error = new Error('No Access token available');

      return revokeTokenCallback(_error);
    }

    var refreshToken = this.signInUserSession.getRefreshToken().getToken();
    var accessToken = this.signInUserSession.getAccessToken();

    if (this.isSessionRevocable(accessToken)) {
      if (refreshToken) {
        return this.revokeToken({
          token: refreshToken,
          callback: revokeTokenCallback
        });
      }
    }

    revokeTokenCallback();
  };

  _proto.isSessionRevocable = function isSessionRevocable(token) {
    if (token && typeof token.decodePayload === 'function') {
      try {
        var _token$decodePayload = token.decodePayload(),
            origin_jti = _token$decodePayload.origin_jti;

        return !!origin_jti;
      } catch (err) {// Nothing to do, token doesnt have origin_jti claim
      }
    }

    return false;
  };

  _proto.cleanClientData = function cleanClientData() {
    this.signInUserSession = null;
    this.clearCachedUser();
  };

  _proto.revokeToken = function revokeToken(_ref2) {
    var token = _ref2.token,
        callback = _ref2.callback;
    this.client.requestWithRetry('RevokeToken', {
      Token: token,
      ClientId: this.pool.getClientId()
    }, function (err) {
      if (err) {
        return callback(err);
      }

      callback();
    });
  }
  /**
   * This is used by a user trying to select a given MFA
   * @param {string} answerChallenge the mfa the user wants
   * @param {nodeCallback<string>} callback Called on success or error.
   * @returns {void}
   */
  ;

  _proto.sendMFASelectionAnswer = function sendMFASelectionAnswer(answerChallenge, callback) {
    var _this19 = this;

    var challengeResponses = {};
    challengeResponses.USERNAME = this.username;
    challengeResponses.ANSWER = answerChallenge;
    var jsonReq = {
      ChallengeName: 'SELECT_MFA_TYPE',
      ChallengeResponses: challengeResponses,
      ClientId: this.pool.getClientId(),
      Session: this.Session
    };

    if (this.getUserContextData()) {
      jsonReq.UserContextData = this.getUserContextData();
    }

    this.client.request('RespondToAuthChallenge', jsonReq, function (err, data) {
      if (err) {
        return callback.onFailure(err);
      }

      _this19.Session = data.Session;

      if (answerChallenge === 'SMS_MFA') {
        return callback.mfaRequired(data.ChallengeName, data.ChallengeParameters);
      }

      if (answerChallenge === 'SOFTWARE_TOKEN_MFA') {
        return callback.totpRequired(data.ChallengeName, data.ChallengeParameters);
      }

      return undefined;
    });
  }
  /**
   * This returns the user context data for advanced security feature.
   * @returns {string} the user context data from CognitoUserPool
   */
  ;

  _proto.getUserContextData = function getUserContextData() {
    var pool = this.pool;
    return pool.getUserContextData(this.username);
  }
  /**
   * This is used by an authenticated or a user trying to authenticate to associate a TOTP MFA
   * @param {nodeCallback<string>} callback Called on success or error.
   * @returns {void}
   */
  ;

  _proto.associateSoftwareToken = function associateSoftwareToken(callback) {
    var _this20 = this;

    if (!(this.signInUserSession != null && this.signInUserSession.isValid())) {
      this.client.request('AssociateSoftwareToken', {
        Session: this.Session
      }, function (err, data) {
        if (err) {
          return callback.onFailure(err);
        }

        _this20.Session = data.Session;
        return callback.associateSecretCode(data.SecretCode);
      });
    } else {
      this.client.request('AssociateSoftwareToken', {
        AccessToken: this.signInUserSession.getAccessToken().getJwtToken()
      }, function (err, data) {
        if (err) {
          return callback.onFailure(err);
        }

        return callback.associateSecretCode(data.SecretCode);
      });
    }
  }
  /**
   * This is used by an authenticated or a user trying to authenticate to verify a TOTP MFA
   * @param {string} totpCode The MFA code entered by the user.
   * @param {string} friendlyDeviceName The device name we are assigning to the device.
   * @param {nodeCallback<string>} callback Called on success or error.
   * @returns {void}
   */
  ;

  _proto.verifySoftwareToken = function verifySoftwareToken(totpCode, friendlyDeviceName, callback) {
    var _this21 = this;

    if (!(this.signInUserSession != null && this.signInUserSession.isValid())) {
      this.client.request('VerifySoftwareToken', {
        Session: this.Session,
        UserCode: totpCode,
        FriendlyDeviceName: friendlyDeviceName
      }, function (err, data) {
        if (err) {
          return callback.onFailure(err);
        }

        _this21.Session = data.Session;
        var challengeResponses = {};
        challengeResponses.USERNAME = _this21.username;
        var jsonReq = {
          ChallengeName: 'MFA_SETUP',
          ClientId: _this21.pool.getClientId(),
          ChallengeResponses: challengeResponses,
          Session: _this21.Session
        };

        if (_this21.getUserContextData()) {
          jsonReq.UserContextData = _this21.getUserContextData();
        }

        _this21.client.request('RespondToAuthChallenge', jsonReq, function (errRespond, dataRespond) {
          if (errRespond) {
            return callback.onFailure(errRespond);
          }

          _this21.signInUserSession = _this21.getCognitoUserSession(dataRespond.AuthenticationResult);

          _this21.cacheTokens();

          return callback.onSuccess(_this21.signInUserSession);
        });

        return undefined;
      });
    } else {
      this.client.request('VerifySoftwareToken', {
        AccessToken: this.signInUserSession.getAccessToken().getJwtToken(),
        UserCode: totpCode,
        FriendlyDeviceName: friendlyDeviceName
      }, function (err, data) {
        if (err) {
          return callback.onFailure(err);
        }

        return callback.onSuccess(data);
      });
    }
  };

  return CognitoUser;
}();

function require$$0(e,n){return n=n||{},new Promise(function(t,r){var s=new XMLHttpRequest,o=[],u=[],i={},a=function(){return {ok:2==(s.status/100|0),statusText:s.statusText,status:s.status,url:s.responseURL,text:function(){return Promise.resolve(s.responseText)},json:function(){return Promise.resolve(s.responseText).then(JSON.parse)},blob:function(){return Promise.resolve(new Blob([s.response]))},clone:a,headers:{keys:function(){return o},entries:function(){return u},get:function(e){return i[e.toLowerCase()]},has:function(e){return e.toLowerCase()in i}}}};for(var l in s.open(n.method||"get",e,!0),s.onload=function(){s.getAllResponseHeaders().replace(/^(.*?):[^\S\n]*([\s\S]*?)$/gm,function(e,n,t){o.push(n=n.toLowerCase()),u.push([n,t]),i[n]=i[n]?i[n]+","+t:t;}),t(a());},s.onerror=r,s.withCredentials="include"==n.credentials,n.headers)s.setRequestHeader(l,n.headers[l]);s.send(n.body||null);})}

self.fetch || (self.fetch = require$$0.default || require$$0);

// generated by genversion
var version = '5.0.4';

/*!
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: Apache-2.0
 */
var BASE_USER_AGENT = "aws-amplify/" + version;
var Platform = {
  userAgent: BASE_USER_AGENT + " js",
  product: '',
  navigator: null,
  isReactNative: false
};

if (typeof navigator !== 'undefined' && navigator.product) {
  Platform.product = navigator.product || '';
  Platform.navigator = navigator || null;

  switch (navigator.product) {
    case 'ReactNative':
      Platform.userAgent = BASE_USER_AGENT + " react-native";
      Platform.isReactNative = true;
      break;

    default:
      Platform.userAgent = BASE_USER_AGENT + " js";
      Platform.isReactNative = false;
      break;
  }
}

var getUserAgent = function getUserAgent() {
  return Platform.userAgent;
};

function UserAgent() {} // public


UserAgent.prototype.userAgent = getUserAgent();

function _inheritsLoose(subClass, superClass) { subClass.prototype = Object.create(superClass.prototype); subClass.prototype.constructor = subClass; _setPrototypeOf(subClass, superClass); }

function _wrapNativeSuper(Class) { var _cache = typeof Map === "function" ? new Map() : undefined; _wrapNativeSuper = function _wrapNativeSuper(Class) { if (Class === null || !_isNativeFunction(Class)) return Class; if (typeof Class !== "function") { throw new TypeError("Super expression must either be null or a function"); } if (typeof _cache !== "undefined") { if (_cache.has(Class)) return _cache.get(Class); _cache.set(Class, Wrapper); } function Wrapper() { return _construct(Class, arguments, _getPrototypeOf(this).constructor); } Wrapper.prototype = Object.create(Class.prototype, { constructor: { value: Wrapper, enumerable: false, writable: true, configurable: true } }); return _setPrototypeOf(Wrapper, Class); }; return _wrapNativeSuper(Class); }

function _construct(Parent, args, Class) { if (_isNativeReflectConstruct()) { _construct = Reflect.construct; } else { _construct = function _construct(Parent, args, Class) { var a = [null]; a.push.apply(a, args); var Constructor = Function.bind.apply(Parent, a); var instance = new Constructor(); if (Class) _setPrototypeOf(instance, Class.prototype); return instance; }; } return _construct.apply(null, arguments); }

function _isNativeReflectConstruct() { if (typeof Reflect === "undefined" || !Reflect.construct) return false; if (Reflect.construct.sham) return false; if (typeof Proxy === "function") return true; try { Boolean.prototype.valueOf.call(Reflect.construct(Boolean, [], function () {})); return true; } catch (e) { return false; } }

function _isNativeFunction(fn) { return Function.toString.call(fn).indexOf("[native code]") !== -1; }

function _setPrototypeOf(o, p) { _setPrototypeOf = Object.setPrototypeOf || function _setPrototypeOf(o, p) { o.__proto__ = p; return o; }; return _setPrototypeOf(o, p); }

function _getPrototypeOf(o) { _getPrototypeOf = Object.setPrototypeOf ? Object.getPrototypeOf : function _getPrototypeOf(o) { return o.__proto__ || Object.getPrototypeOf(o); }; return _getPrototypeOf(o); }

var CognitoError = /*#__PURE__*/function (_Error) {
  _inheritsLoose(CognitoError, _Error);

  function CognitoError(message, code, name, statusCode) {
    var _this;

    _this = _Error.call(this, message) || this;
    _this.code = code;
    _this.name = name;
    _this.statusCode = statusCode;
    return _this;
  }

  return CognitoError;
}( /*#__PURE__*/_wrapNativeSuper(Error));
/** @class */


var Client = /*#__PURE__*/function () {
  /**
   * Constructs a new AWS Cognito Identity Provider client object
   * @param {string} region AWS region
   * @param {string} endpoint endpoint
   * @param {object} fetchOptions options for fetch API (only credentials is supported)
   */
  function Client(region, endpoint, fetchOptions) {
    this.endpoint = endpoint || "https://cognito-idp." + region + ".amazonaws.com/";

    var _ref = fetchOptions || {},
        credentials = _ref.credentials;

    this.fetchOptions = credentials ? {
      credentials: credentials
    } : {};
  }
  /**
   * Makes an unauthenticated request on AWS Cognito Identity Provider API
   * using fetch
   * @param {string} operation API operation
   * @param {object} params Input parameters
   * @returns Promise<object>
   */


  var _proto = Client.prototype;

  _proto.promisifyRequest = function promisifyRequest(operation, params) {
    var _this2 = this;

    return new Promise(function (resolve, reject) {
      _this2.request(operation, params, function (err, data) {
        if (err) {
          reject(new CognitoError(err.message, err.code, err.name, err.statusCode));
        } else {
          resolve(data);
        }
      });
    });
  };

  _proto.requestWithRetry = function requestWithRetry(operation, params, callback) {
    var _this3 = this;

    var MAX_DELAY_IN_MILLIS = 5 * 1000;
    jitteredExponentialRetry(function (p) {
      return new Promise(function (res, rej) {
        _this3.request(operation, p, function (error, result) {
          if (error) {
            rej(error);
          } else {
            res(result);
          }
        });
      });
    }, [params], MAX_DELAY_IN_MILLIS).then(function (result) {
      return callback(null, result);
    })["catch"](function (error) {
      return callback(error);
    });
  }
  /**
   * Makes an unauthenticated request on AWS Cognito Identity Provider API
   * using fetch
   * @param {string} operation API operation
   * @param {object} params Input parameters
   * @param {function} callback Callback called when a response is returned
   * @returns {void}
   */
  ;

  _proto.request = function request(operation, params, callback) {
    var headers = {
      'Content-Type': 'application/x-amz-json-1.1',
      'X-Amz-Target': "AWSCognitoIdentityProviderService." + operation,
      'X-Amz-User-Agent': UserAgent.prototype.userAgent
    };
    var options = Object.assign({}, this.fetchOptions, {
      headers: headers,
      method: 'POST',
      mode: 'cors',
      cache: 'no-cache',
      body: JSON.stringify(params)
    });
    var response;
    fetch(this.endpoint, options).then(function (resp) {
      response = resp;
      return resp;
    }, function (err) {
      // If error happens here, the request failed
      // if it is TypeError throw network error
      if (err instanceof TypeError) {
        throw new Error('Network error');
      }

      throw err;
    }).then(function (resp) {
      return resp.json()["catch"](function () {
        return {};
      });
    }).then(function (data) {
      // return parsed body stream
      if (response.ok) return callback(null, data);
      // eslint-disable-next-line no-underscore-dangle

      var code = (data.__type || data.code).split('#').pop();
      var error = new Error(data.message || data.Message || null);
      error.name = code;
      error.code = code;
      return callback(error);
    })["catch"](function (err) {
      // first check if we have a service error
      if (response && response.headers && response.headers.get('x-amzn-errortype')) {
        try {
          var code = response.headers.get('x-amzn-errortype').split(':')[0];
          var error = new Error(response.status ? response.status.toString() : null);
          error.code = code;
          error.name = code;
          error.statusCode = response.status;
          return callback(error);
        } catch (ex) {
          return callback(err);
        } // otherwise check if error is Network error

      } else if (err instanceof Error && err.message === 'Network error') {
        err.code = 'NetworkError';
      }

      return callback(err);
    });
  };

  return Client;
}();
var logger$3 = {
  debug: function debug() {// Intentionally blank. This package doesn't have logging
  }
};

var isNonRetryableError = function isNonRetryableError(obj) {
  var key = 'nonRetryable';
  return obj && obj[key];
};

function retry(functionToRetry, args, delayFn, attempt) {
  if (attempt === void 0) {
    attempt = 1;
  }

  if (typeof functionToRetry !== 'function') {
    throw Error('functionToRetry must be a function');
  }

  logger$3.debug(functionToRetry.name + " attempt #" + attempt + " with args: " + JSON.stringify(args));
  return functionToRetry.apply(void 0, args)["catch"](function (err) {
    logger$3.debug("error on " + functionToRetry.name, err);

    if (isNonRetryableError(err)) {
      logger$3.debug(functionToRetry.name + " non retryable error", err);
      throw err;
    }

    var retryIn = delayFn(attempt, args, err);
    logger$3.debug(functionToRetry.name + " retrying in " + retryIn + " ms");

    if (retryIn !== false) {
      return new Promise(function (res) {
        return setTimeout(res, retryIn);
      }).then(function () {
        return retry(functionToRetry, args, delayFn, attempt + 1);
      });
    } else {
      throw err;
    }
  });
}

function jitteredBackoff(maxDelayMs) {
  var BASE_TIME_MS = 100;
  var JITTER_FACTOR = 100;
  return function (attempt) {
    var delay = Math.pow(2, attempt) * BASE_TIME_MS + JITTER_FACTOR * Math.random();
    return delay > maxDelayMs ? false : delay;
  };
}

var MAX_DELAY_MS = 5 * 60 * 1000;

function jitteredExponentialRetry(functionToRetry, args, maxDelayMs) {
  if (maxDelayMs === void 0) {
    maxDelayMs = MAX_DELAY_MS;
  }

  return retry(functionToRetry, args, jitteredBackoff(maxDelayMs));
}

/*!
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: Apache-2.0
 */
var USER_POOL_ID_MAX_LENGTH = 55;
/** @class */

var CognitoUserPool = /*#__PURE__*/function () {
  /**
   * Constructs a new CognitoUserPool object
   * @param {object} data Creation options.
   * @param {string} data.UserPoolId Cognito user pool id.
   * @param {string} data.ClientId User pool application client id.
   * @param {string} data.endpoint Optional custom service endpoint.
   * @param {object} data.fetchOptions Optional options for fetch API.
   *        (only credentials option is supported)
   * @param {object} data.Storage Optional storage object.
   * @param {boolean} data.AdvancedSecurityDataCollectionFlag Optional:
   *        boolean flag indicating if the data collection is enabled
   *        to support cognito advanced security features. By default, this
   *        flag is set to true.
   */
  function CognitoUserPool(data, wrapRefreshSessionCallback) {
    var _ref = data || {},
        UserPoolId = _ref.UserPoolId,
        ClientId = _ref.ClientId,
        endpoint = _ref.endpoint,
        fetchOptions = _ref.fetchOptions,
        AdvancedSecurityDataCollectionFlag = _ref.AdvancedSecurityDataCollectionFlag;

    if (!UserPoolId || !ClientId) {
      throw new Error('Both UserPoolId and ClientId are required.');
    }

    if (UserPoolId.length > USER_POOL_ID_MAX_LENGTH || !/^[\w-]+_[0-9a-zA-Z]+$/.test(UserPoolId)) {
      throw new Error('Invalid UserPoolId format.');
    }

    var region = UserPoolId.split('_')[0];
    this.userPoolId = UserPoolId;
    this.clientId = ClientId;
    this.client = new Client(region, endpoint, fetchOptions);
    /**
     * By default, AdvancedSecurityDataCollectionFlag is set to true,
     * if no input value is provided.
     */

    this.advancedSecurityDataCollectionFlag = AdvancedSecurityDataCollectionFlag !== false;
    this.storage = data.Storage || new StorageHelper().getStorage();

    if (wrapRefreshSessionCallback) {
      this.wrapRefreshSessionCallback = wrapRefreshSessionCallback;
    }
  }
  /**
   * @returns {string} the user pool id
   */


  var _proto = CognitoUserPool.prototype;

  _proto.getUserPoolId = function getUserPoolId() {
    return this.userPoolId;
  }
  /**
   * @returns {string} the client id
   */
  ;

  _proto.getClientId = function getClientId() {
    return this.clientId;
  }
  /**
   * @typedef {object} SignUpResult
   * @property {CognitoUser} user New user.
   * @property {bool} userConfirmed If the user is already confirmed.
   */

  /**
   * method for signing up a user
   * @param {string} username User's username.
   * @param {string} password Plain-text initial password entered by user.
   * @param {(AttributeArg[])=} userAttributes New user attributes.
   * @param {(AttributeArg[])=} validationData Application metadata.
   * @param {(AttributeArg[])=} clientMetadata Client metadata.
   * @param {nodeCallback<SignUpResult>} callback Called on error or with the new user.
   * @param {ClientMetadata} clientMetadata object which is passed from client to Cognito Lambda trigger
   * @returns {void}
   */
  ;

  _proto.signUp = function signUp(username, password, userAttributes, validationData, callback, clientMetadata) {
    var _this = this;

    var jsonReq = {
      ClientId: this.clientId,
      Username: username,
      Password: password,
      UserAttributes: userAttributes,
      ValidationData: validationData,
      ClientMetadata: clientMetadata
    };

    if (this.getUserContextData(username)) {
      jsonReq.UserContextData = this.getUserContextData(username);
    }

    this.client.request('SignUp', jsonReq, function (err, data) {
      if (err) {
        return callback(err, null);
      }

      var cognitoUser = {
        Username: username,
        Pool: _this,
        Storage: _this.storage
      };
      var returnData = {
        user: new CognitoUser(cognitoUser),
        userConfirmed: data.UserConfirmed,
        userSub: data.UserSub,
        codeDeliveryDetails: data.CodeDeliveryDetails
      };
      return callback(null, returnData);
    });
  }
  /**
   * method for getting the current user of the application from the local storage
   *
   * @returns {CognitoUser} the user retrieved from storage
   */
  ;

  _proto.getCurrentUser = function getCurrentUser() {
    var lastUserKey = "CognitoIdentityServiceProvider." + this.clientId + ".LastAuthUser";
    var lastAuthUser = this.storage.getItem(lastUserKey);

    if (lastAuthUser) {
      var cognitoUser = {
        Username: lastAuthUser,
        Pool: this,
        Storage: this.storage
      };
      return new CognitoUser(cognitoUser);
    }

    return null;
  }
  /**
   * This method returns the encoded data string used for cognito advanced security feature.
   * This would be generated only when developer has included the JS used for collecting the
   * data on their client. Please refer to documentation to know more about using AdvancedSecurity
   * features
   * @param {string} username the username for the context data
   * @returns {string} the user context data
   **/
  ;

  _proto.getUserContextData = function getUserContextData(username) {
    if (typeof AmazonCognitoAdvancedSecurityData === 'undefined') {
      return undefined;
    }
    /* eslint-disable */


    var amazonCognitoAdvancedSecurityDataConst = AmazonCognitoAdvancedSecurityData;
    /* eslint-enable */

    if (this.advancedSecurityDataCollectionFlag) {
      var advancedSecurityData = amazonCognitoAdvancedSecurityDataConst.getData(username, this.userPoolId, this.clientId);

      if (advancedSecurityData) {
        var userContextData = {
          EncodedData: advancedSecurityData
        };
        return userContextData;
      }
    }

    return {};
  };

  return CognitoUserPool;
}();

var js_cookie = createCommonjsModule(function (module, exports) {
(function (factory) {
	var registeredInModuleLoader;
	{
		module.exports = factory();
		registeredInModuleLoader = true;
	}
	if (!registeredInModuleLoader) {
		var OldCookies = window.Cookies;
		var api = window.Cookies = factory();
		api.noConflict = function () {
			window.Cookies = OldCookies;
			return api;
		};
	}
}(function () {
	function extend () {
		var i = 0;
		var result = {};
		for (; i < arguments.length; i++) {
			var attributes = arguments[ i ];
			for (var key in attributes) {
				result[key] = attributes[key];
			}
		}
		return result;
	}

	function decode (s) {
		return s.replace(/(%[0-9A-Z]{2})+/g, decodeURIComponent);
	}

	function init (converter) {
		function api() {}

		function set (key, value, attributes) {
			if (typeof document === 'undefined') {
				return;
			}

			attributes = extend({
				path: '/'
			}, api.defaults, attributes);

			if (typeof attributes.expires === 'number') {
				attributes.expires = new Date(new Date() * 1 + attributes.expires * 864e+5);
			}

			// We're using "expires" because "max-age" is not supported by IE
			attributes.expires = attributes.expires ? attributes.expires.toUTCString() : '';

			try {
				var result = JSON.stringify(value);
				if (/^[\{\[]/.test(result)) {
					value = result;
				}
			} catch (e) {}

			value = converter.write ?
				converter.write(value, key) :
				encodeURIComponent(String(value))
					.replace(/%(23|24|26|2B|3A|3C|3E|3D|2F|3F|40|5B|5D|5E|60|7B|7D|7C)/g, decodeURIComponent);

			key = encodeURIComponent(String(key))
				.replace(/%(23|24|26|2B|5E|60|7C)/g, decodeURIComponent)
				.replace(/[\(\)]/g, escape);

			var stringifiedAttributes = '';
			for (var attributeName in attributes) {
				if (!attributes[attributeName]) {
					continue;
				}
				stringifiedAttributes += '; ' + attributeName;
				if (attributes[attributeName] === true) {
					continue;
				}

				// Considers RFC 6265 section 5.2:
				// ...
				// 3.  If the remaining unparsed-attributes contains a %x3B (";")
				//     character:
				// Consume the characters of the unparsed-attributes up to,
				// not including, the first %x3B (";") character.
				// ...
				stringifiedAttributes += '=' + attributes[attributeName].split(';')[0];
			}

			return (document.cookie = key + '=' + value + stringifiedAttributes);
		}

		function get (key, json) {
			if (typeof document === 'undefined') {
				return;
			}

			var jar = {};
			// To prevent the for loop in the first place assign an empty array
			// in case there are no cookies at all.
			var cookies = document.cookie ? document.cookie.split('; ') : [];
			var i = 0;

			for (; i < cookies.length; i++) {
				var parts = cookies[i].split('=');
				var cookie = parts.slice(1).join('=');

				if (!json && cookie.charAt(0) === '"') {
					cookie = cookie.slice(1, -1);
				}

				try {
					var name = decode(parts[0]);
					cookie = (converter.read || converter)(cookie, name) ||
						decode(cookie);

					if (json) {
						try {
							cookie = JSON.parse(cookie);
						} catch (e) {}
					}

					jar[name] = cookie;

					if (key === name) {
						break;
					}
				} catch (e) {}
			}

			return key ? jar[key] : jar;
		}

		api.set = set;
		api.get = function (key) {
			return get(key, false /* read as raw */);
		};
		api.getJSON = function (key) {
			return get(key, true /* read as json */);
		};
		api.remove = function (key, attributes) {
			set(key, '', extend(attributes, {
				expires: -1
			}));
		};

		api.defaults = {};

		api.withConverter = init;

		return api;
	}

	return init(function () {});
}));
});

/** @class */

var CookieStorage = /*#__PURE__*/function () {
  /**
   * Constructs a new CookieStorage object
   * @param {object} data Creation options.
   * @param {string} data.domain Cookies domain (mandatory).
   * @param {string} data.path Cookies path (default: '/')
   * @param {integer} data.expires Cookie expiration (in days, default: 365)
   * @param {boolean} data.secure Cookie secure flag (default: true)
   * @param {string} data.sameSite Cookie request behaviour (default: null)
   */
  function CookieStorage(data) {
    if (data.domain) {
      this.domain = data.domain;
    } else {
      throw new Error('The domain of cookieStorage can not be undefined.');
    }

    if (data.path) {
      this.path = data.path;
    } else {
      this.path = '/';
    }

    if (Object.prototype.hasOwnProperty.call(data, 'expires')) {
      this.expires = data.expires;
    } else {
      this.expires = 365;
    }

    if (Object.prototype.hasOwnProperty.call(data, 'secure')) {
      this.secure = data.secure;
    } else {
      this.secure = true;
    }

    if (Object.prototype.hasOwnProperty.call(data, 'sameSite')) {
      if (!['strict', 'lax', 'none'].includes(data.sameSite)) {
        throw new Error('The sameSite value of cookieStorage must be "lax", "strict" or "none".');
      }

      if (data.sameSite === 'none' && !this.secure) {
        throw new Error('sameSite = None requires the Secure attribute in latest browser versions.');
      }

      this.sameSite = data.sameSite;
    } else {
      this.sameSite = null;
    }
  }
  /**
   * This is used to set a specific item in storage
   * @param {string} key - the key for the item
   * @param {object} value - the value
   * @returns {string} value that was set
   */


  var _proto = CookieStorage.prototype;

  _proto.setItem = function setItem(key, value) {
    var options = {
      path: this.path,
      expires: this.expires,
      domain: this.domain,
      secure: this.secure
    };

    if (this.sameSite) {
      options.sameSite = this.sameSite;
    }

    js_cookie.set(key, value, options);
    return js_cookie.get(key);
  }
  /**
   * This is used to get a specific key from storage
   * @param {string} key - the key for the item
   * This is used to clear the storage
   * @returns {string} the data item
   */
  ;

  _proto.getItem = function getItem(key) {
    return js_cookie.get(key);
  }
  /**
   * This is used to remove an item from storage
   * @param {string} key - the key being set
   * @returns {string} value - value that was deleted
   */
  ;

  _proto.removeItem = function removeItem(key) {
    var options = {
      path: this.path,
      expires: this.expires,
      domain: this.domain,
      secure: this.secure
    };

    if (this.sameSite) {
      options.sameSite = this.sameSite;
    }

    return js_cookie.remove(key, options);
  }
  /**
   * This is used to clear the storage of optional
   * items that were previously set
   * @returns {} an empty object
   */
  ;

  _proto.clear = function clear() {
    var cookies = js_cookie.get();
    var numKeys = Object.keys(cookies).length;

    for (var index = 0; index < numKeys; ++index) {
      this.removeItem(Object.keys(cookies)[index]);
    }

    return {};
  };

  return CookieStorage;
}();

/*
 * Copyright 2017-2017 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"). You may not use this file except in compliance with
 * the License. A copy of the License is located at
 *
 *     http://aws.amazon.com/apache2.0/
 *
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
 * and limitations under the License.
 */
var SELF = '_self';
var launchUri = function (url) {
    var windowProxy = window.open(url, SELF);
    if (windowProxy) {
        return Promise.resolve(windowProxy);
    }
    else {
        return Promise.reject();
    }
};

/*
 * Copyright 2017-2017 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"). You may not use this file except in compliance with
 * the License. A copy of the License is located at
 *
 *     http://aws.amazon.com/apache2.0/
 *
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
 * and limitations under the License.
 */
var setState = function (state) {
    window.sessionStorage.setItem('oauth_state', state);
};
var getState = function () {
    var oauth_state = window.sessionStorage.getItem('oauth_state');
    window.sessionStorage.removeItem('oauth_state');
    return oauth_state;
};
var setPKCE = function (private_key) {
    window.sessionStorage.setItem('ouath_pkce_key', private_key);
};
var getPKCE = function () {
    var ouath_pkce_key = window.sessionStorage.getItem('ouath_pkce_key');
    window.sessionStorage.removeItem('ouath_pkce_key');
    return ouath_pkce_key;
};

/*
 * Copyright 2017-2017 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"). You may not use this file except in compliance with
 * the License. A copy of the License is located at
 *
 *     http://aws.amazon.com/apache2.0/
 *
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
 * and limitations under the License.
 */
var __assign$1 = (undefined && undefined.__assign) || function () {
    __assign$1 = Object.assign || function(t) {
        for (var s, i = 1, n = arguments.length; i < n; i++) {
            s = arguments[i];
            for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
                t[p] = s[p];
        }
        return t;
    };
    return __assign$1.apply(this, arguments);
};
var __awaiter$1 = (undefined && undefined.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator$1 = (undefined && undefined.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
var __read$1 = (undefined && undefined.__read) || function (o, n) {
    var m = typeof Symbol === "function" && o[Symbol.iterator];
    if (!m) return o;
    var i = m.call(o), r, ar = [], e;
    try {
        while ((n === void 0 || n-- > 0) && !(r = i.next()).done) ar.push(r.value);
    }
    catch (error) { e = { error: error }; }
    finally {
        try {
            if (r && !r.done && (m = i["return"])) m.call(i);
        }
        finally { if (e) throw e.error; }
    }
    return ar;
};
var AMPLIFY_SYMBOL$1 = (typeof Symbol !== 'undefined' &&
    typeof Symbol.for === 'function'
    ? Symbol.for('amplify_default')
    : '@@amplify_default');
var dispatchAuthEvent$1 = function (event, data, message) {
    Hub.dispatch('auth', { event: event, data: data, message: message }, 'Auth', AMPLIFY_SYMBOL$1);
};
var logger$2 = new ConsoleLogger('OAuth');
var OAuth = /** @class */ (function () {
    function OAuth(_a) {
        var config = _a.config, cognitoClientId = _a.cognitoClientId, _b = _a.scopes, scopes = _b === void 0 ? [] : _b;
        this._urlOpener = config.urlOpener || launchUri;
        this._config = config;
        this._cognitoClientId = cognitoClientId;
        if (!this.isValidScopes(scopes))
            throw Error('scopes must be a String Array');
        this._scopes = scopes;
    }
    OAuth.prototype.isValidScopes = function (scopes) {
        return (Array.isArray(scopes) && scopes.every(function (scope) { return typeof scope === 'string'; }));
    };
    OAuth.prototype.oauthSignIn = function (responseType, domain, redirectSignIn, clientId, provider, customState) {
        if (responseType === void 0) { responseType = 'code'; }
        if (provider === void 0) { provider = CognitoHostedUIIdentityProvider.Cognito; }
        var generatedState = this._generateState(32);
        /* encodeURIComponent is not URL safe, use urlSafeEncode instead. Cognito
        single-encodes/decodes url on first sign in and double-encodes/decodes url
        when user already signed in. Using encodeURIComponent, Base32, Base64 add
        characters % or = which on further encoding becomes unsafe. '=' create issue
        for parsing query params.
        Refer: https://github.com/aws-amplify/amplify-js/issues/5218 */
        var state = customState
            ? generatedState + "-" + urlSafeEncode(customState)
            : generatedState;
        setState(state);
        var pkce_key = this._generateRandom(128);
        setPKCE(pkce_key);
        var code_challenge = this._generateChallenge(pkce_key);
        var code_challenge_method = 'S256';
        var scopesString = this._scopes.join(' ');
        var queryString = Object.entries(__assign$1(__assign$1({ redirect_uri: redirectSignIn, response_type: responseType, client_id: clientId, identity_provider: provider, scope: scopesString, state: state }, (responseType === 'code' ? { code_challenge: code_challenge } : {})), (responseType === 'code' ? { code_challenge_method: code_challenge_method } : {})))
            .map(function (_a) {
            var _b = __read$1(_a, 2), k = _b[0], v = _b[1];
            return encodeURIComponent(k) + "=" + encodeURIComponent(v);
        })
            .join('&');
        var URL = "https://" + domain + "/oauth2/authorize?" + queryString;
        logger$2.debug("Redirecting to " + URL);
        this._urlOpener(URL, redirectSignIn);
    };
    OAuth.prototype._handleCodeFlow = function (currentUrl) {
        return __awaiter$1(this, void 0, void 0, function () {
            var code, currentUrlPathname, redirectSignInPathname, oAuthTokenEndpoint, client_id, redirect_uri, code_verifier, oAuthTokenBody, body, _a, access_token, refresh_token, id_token, error;
            return __generator$1(this, function (_b) {
                switch (_b.label) {
                    case 0:
                        code = (urlParse(currentUrl).query || '')
                            .split('&')
                            .map(function (pairings) { return pairings.split('='); })
                            .reduce(function (accum, _a) {
                            var _b;
                            var _c = __read$1(_a, 2), k = _c[0], v = _c[1];
                            return (__assign$1(__assign$1({}, accum), (_b = {}, _b[k] = v, _b)));
                        }, { code: undefined }).code;
                        currentUrlPathname = urlParse(currentUrl).pathname || '/';
                        redirectSignInPathname = urlParse(this._config.redirectSignIn).pathname || '/';
                        if (!code || currentUrlPathname !== redirectSignInPathname) {
                            return [2 /*return*/];
                        }
                        oAuthTokenEndpoint = 'https://' + this._config.domain + '/oauth2/token';
                        dispatchAuthEvent$1('codeFlow', {}, "Retrieving tokens from " + oAuthTokenEndpoint);
                        client_id = isCognitoHostedOpts(this._config)
                            ? this._cognitoClientId
                            : this._config.clientID;
                        redirect_uri = isCognitoHostedOpts(this._config)
                            ? this._config.redirectSignIn
                            : this._config.redirectUri;
                        code_verifier = getPKCE();
                        oAuthTokenBody = __assign$1({ grant_type: 'authorization_code', code: code,
                            client_id: client_id,
                            redirect_uri: redirect_uri }, (code_verifier ? { code_verifier: code_verifier } : {}));
                        logger$2.debug("Calling token endpoint: " + oAuthTokenEndpoint + " with", oAuthTokenBody);
                        body = Object.entries(oAuthTokenBody)
                            .map(function (_a) {
                            var _b = __read$1(_a, 2), k = _b[0], v = _b[1];
                            return encodeURIComponent(k) + "=" + encodeURIComponent(v);
                        })
                            .join('&');
                        return [4 /*yield*/, fetch(oAuthTokenEndpoint, {
                                method: 'POST',
                                headers: {
                                    'Content-Type': 'application/x-www-form-urlencoded',
                                },
                                body: body,
                            })];
                    case 1: return [4 /*yield*/, (_b.sent()).json()];
                    case 2:
                        _a = _b.sent(), access_token = _a.access_token, refresh_token = _a.refresh_token, id_token = _a.id_token, error = _a.error;
                        if (error) {
                            throw new Error(error);
                        }
                        return [2 /*return*/, {
                                accessToken: access_token,
                                refreshToken: refresh_token,
                                idToken: id_token,
                            }];
                }
            });
        });
    };
    OAuth.prototype._handleImplicitFlow = function (currentUrl) {
        return __awaiter$1(this, void 0, void 0, function () {
            var _a, id_token, access_token;
            return __generator$1(this, function (_b) {
                _a = (urlParse(currentUrl).hash || '#')
                    .substr(1) // Remove # from returned code
                    .split('&')
                    .map(function (pairings) { return pairings.split('='); })
                    .reduce(function (accum, _a) {
                    var _b;
                    var _c = __read$1(_a, 2), k = _c[0], v = _c[1];
                    return (__assign$1(__assign$1({}, accum), (_b = {}, _b[k] = v, _b)));
                }, {
                    id_token: undefined,
                    access_token: undefined,
                }), id_token = _a.id_token, access_token = _a.access_token;
                dispatchAuthEvent$1('implicitFlow', {}, "Got tokens from " + currentUrl);
                logger$2.debug("Retrieving implicit tokens from " + currentUrl + " with");
                return [2 /*return*/, {
                        accessToken: access_token,
                        idToken: id_token,
                        refreshToken: null,
                    }];
            });
        });
    };
    OAuth.prototype.handleAuthResponse = function (currentUrl) {
        return __awaiter$1(this, void 0, void 0, function () {
            var urlParams, error, error_description, state, _a, _b, e_1;
            return __generator$1(this, function (_c) {
                switch (_c.label) {
                    case 0:
                        _c.trys.push([0, 5, , 6]);
                        urlParams = currentUrl
                            ? __assign$1(__assign$1({}, (urlParse(currentUrl).hash || '#')
                                .substr(1)
                                .split('&')
                                .map(function (entry) { return entry.split('='); })
                                .reduce(function (acc, _a) {
                                var _b = __read$1(_a, 2), k = _b[0], v = _b[1];
                                return ((acc[k] = v), acc);
                            }, {})), (urlParse(currentUrl).query || '')
                                .split('&')
                                .map(function (entry) { return entry.split('='); })
                                .reduce(function (acc, _a) {
                                var _b = __read$1(_a, 2), k = _b[0], v = _b[1];
                                return ((acc[k] = v), acc);
                            }, {}))
                            : {};
                        error = urlParams.error, error_description = urlParams.error_description;
                        if (error) {
                            throw new Error(error_description);
                        }
                        state = this._validateState(urlParams);
                        logger$2.debug("Starting " + this._config.responseType + " flow with " + currentUrl);
                        if (!(this._config.responseType === 'code')) return [3 /*break*/, 2];
                        _a = [{}];
                        return [4 /*yield*/, this._handleCodeFlow(currentUrl)];
                    case 1: return [2 /*return*/, __assign$1.apply(void 0, [__assign$1.apply(void 0, _a.concat([(_c.sent())])), { state: state }])];
                    case 2:
                        _b = [{}];
                        return [4 /*yield*/, this._handleImplicitFlow(currentUrl)];
                    case 3: return [2 /*return*/, __assign$1.apply(void 0, [__assign$1.apply(void 0, _b.concat([(_c.sent())])), { state: state }])];
                    case 4: return [3 /*break*/, 6];
                    case 5:
                        e_1 = _c.sent();
                        logger$2.error("Error handling auth response.", e_1);
                        throw e_1;
                    case 6: return [2 /*return*/];
                }
            });
        });
    };
    OAuth.prototype._validateState = function (urlParams) {
        if (!urlParams) {
            return;
        }
        var savedState = getState();
        var returnedState = urlParams.state;
        // This is because savedState only exists if the flow was initiated by Amplify
        if (savedState && savedState !== returnedState) {
            throw new Error('Invalid state in OAuth flow');
        }
        return returnedState;
    };
    OAuth.prototype.signOut = function () {
        return __awaiter$1(this, void 0, void 0, function () {
            var oAuthLogoutEndpoint, client_id, signout_uri;
            return __generator$1(this, function (_a) {
                oAuthLogoutEndpoint = 'https://' + this._config.domain + '/logout?';
                client_id = isCognitoHostedOpts(this._config)
                    ? this._cognitoClientId
                    : this._config.oauth.clientID;
                signout_uri = isCognitoHostedOpts(this._config)
                    ? this._config.redirectSignOut
                    : this._config.returnTo;
                oAuthLogoutEndpoint += Object.entries({
                    client_id: client_id,
                    logout_uri: encodeURIComponent(signout_uri),
                })
                    .map(function (_a) {
                    var _b = __read$1(_a, 2), k = _b[0], v = _b[1];
                    return k + "=" + v;
                })
                    .join('&');
                dispatchAuthEvent$1('oAuthSignOut', { oAuth: 'signOut' }, "Signing out from " + oAuthLogoutEndpoint);
                logger$2.debug("Signing out from " + oAuthLogoutEndpoint);
                return [2 /*return*/, this._urlOpener(oAuthLogoutEndpoint, signout_uri)];
            });
        });
    };
    OAuth.prototype._generateState = function (length) {
        var result = '';
        var i = length;
        var chars = '0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ';
        for (; i > 0; --i)
            result += chars[Math.round(Math.random() * (chars.length - 1))];
        return result;
    };
    OAuth.prototype._generateChallenge = function (code) {
        return this._base64URL(sha256(code));
    };
    OAuth.prototype._base64URL = function (string) {
        return string
            .toString(encBase64)
            .replace(/=/g, '')
            .replace(/\+/g, '-')
            .replace(/\//g, '_');
    };
    OAuth.prototype._generateRandom = function (size) {
        var CHARSET = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~';
        var buffer = new Uint8Array(size);
        if (typeof window !== 'undefined' && !!window.crypto) {
            window.crypto.getRandomValues(buffer);
        }
        else {
            for (var i = 0; i < size; i += 1) {
                buffer[i] = (Math.random() * CHARSET.length) | 0;
            }
        }
        return this._bufferToString(buffer);
    };
    OAuth.prototype._bufferToString = function (buffer) {
        var CHARSET = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
        var state = [];
        for (var i = 0; i < buffer.byteLength; i += 1) {
            var index = buffer[i] % CHARSET.length;
            state.push(CHARSET[index]);
        }
        return state.join('');
    };
    return OAuth;
}());
var OAuth$1 = OAuth;

/*
 * Copyright 2017-2019 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"). You may not use this file except in compliance with
 * the License. A copy of the License is located at
 *
 *     http://aws.amazon.com/apache2.0/
 *
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
 * and limitations under the License.
 */
var urlListener = (function (callback) {
    if (JS.browserOrNode().isBrowser && window.location) {
        var url = window.location.href;
        callback({ url: url });
    }
    else if (JS.browserOrNode().isNode) ;
    else {
        throw new Error('Not supported');
    }
});

var AuthErrorStrings;
(function (AuthErrorStrings) {
    AuthErrorStrings["DEFAULT_MSG"] = "Authentication Error";
    AuthErrorStrings["EMPTY_EMAIL"] = "Email cannot be empty";
    AuthErrorStrings["EMPTY_PHONE"] = "Phone number cannot be empty";
    AuthErrorStrings["EMPTY_USERNAME"] = "Username cannot be empty";
    AuthErrorStrings["INVALID_USERNAME"] = "The username should either be a string or one of the sign in types";
    AuthErrorStrings["EMPTY_PASSWORD"] = "Password cannot be empty";
    AuthErrorStrings["EMPTY_CODE"] = "Confirmation code cannot be empty";
    AuthErrorStrings["SIGN_UP_ERROR"] = "Error creating account";
    AuthErrorStrings["NO_MFA"] = "No valid MFA method provided";
    AuthErrorStrings["INVALID_MFA"] = "Invalid MFA type";
    AuthErrorStrings["EMPTY_CHALLENGE"] = "Challenge response cannot be empty";
    AuthErrorStrings["NO_USER_SESSION"] = "Failed to get the session because the user is empty";
    AuthErrorStrings["NETWORK_ERROR"] = "Network Error";
    AuthErrorStrings["DEVICE_CONFIG"] = "Device tracking has not been configured in this User Pool";
})(AuthErrorStrings || (AuthErrorStrings = {}));

/*
 * Copyright 2019-2020 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"). You may not use this file except in compliance with
 * the License. A copy of the License is located at
 *
 *     http://aws.amazon.com/apache2.0/
 *
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
 * and limitations under the License.
 */
var __extends = (undefined && undefined.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
var logger$1 = new ConsoleLogger('AuthError');
var AuthError = /** @class */ (function (_super) {
    __extends(AuthError, _super);
    function AuthError(type) {
        var _this = this;
        var _a = authErrorMessages[type], message = _a.message, log = _a.log;
        _this = _super.call(this, message) || this;
        // Hack for making the custom error class work when transpiled to es5
        // TODO: Delete the following 2 lines after we change the build target to >= es2015
        _this.constructor = AuthError;
        Object.setPrototypeOf(_this, AuthError.prototype);
        _this.name = 'AuthError';
        _this.log = log || message;
        logger$1.error(_this.log);
        return _this;
    }
    return AuthError;
}(Error));
var NoUserPoolError = /** @class */ (function (_super) {
    __extends(NoUserPoolError, _super);
    function NoUserPoolError(type) {
        var _this = _super.call(this, type) || this;
        // Hack for making the custom error class work when transpiled to es5
        // TODO: Delete the following 2 lines after we change the build target to >= es2015
        _this.constructor = NoUserPoolError;
        Object.setPrototypeOf(_this, NoUserPoolError.prototype);
        _this.name = 'NoUserPoolError';
        return _this;
    }
    return NoUserPoolError;
}(AuthError));
var authErrorMessages = {
    noConfig: {
        message: AuthErrorStrings.DEFAULT_MSG,
        log: "\n            Error: Amplify has not been configured correctly.\n            This error is typically caused by one of the following scenarios:\n\n            1. Make sure you're passing the awsconfig object to Amplify.configure() in your app's entry point\n                See https://aws-amplify.github.io/docs/js/authentication#configure-your-app for more information\n            \n            2. There might be multiple conflicting versions of amplify packages in your node_modules.\n\t\t\t\tRefer to our docs site for help upgrading Amplify packages (https://docs.amplify.aws/lib/troubleshooting/upgrading/q/platform/js)\n        ",
    },
    missingAuthConfig: {
        message: AuthErrorStrings.DEFAULT_MSG,
        log: "\n            Error: Amplify has not been configured correctly. \n            The configuration object is missing required auth properties.\n            This error is typically caused by one of the following scenarios:\n\n            1. Did you run `amplify push` after adding auth via `amplify add auth`?\n                See https://aws-amplify.github.io/docs/js/authentication#amplify-project-setup for more information\n\n            2. This could also be caused by multiple conflicting versions of amplify packages, see (https://docs.amplify.aws/lib/troubleshooting/upgrading/q/platform/js) for help upgrading Amplify packages.\n        ",
    },
    emptyUsername: {
        message: AuthErrorStrings.EMPTY_USERNAME,
    },
    // TODO: should include a list of valid sign-in types
    invalidUsername: {
        message: AuthErrorStrings.INVALID_USERNAME,
    },
    emptyPassword: {
        message: AuthErrorStrings.EMPTY_PASSWORD,
    },
    emptyCode: {
        message: AuthErrorStrings.EMPTY_CODE,
    },
    signUpError: {
        message: AuthErrorStrings.SIGN_UP_ERROR,
        log: 'The first parameter should either be non-null string or object',
    },
    noMFA: {
        message: AuthErrorStrings.NO_MFA,
    },
    invalidMFA: {
        message: AuthErrorStrings.INVALID_MFA,
    },
    emptyChallengeResponse: {
        message: AuthErrorStrings.EMPTY_CHALLENGE,
    },
    noUserSession: {
        message: AuthErrorStrings.NO_USER_SESSION,
    },
    deviceConfig: {
        message: AuthErrorStrings.DEVICE_CONFIG,
    },
    networkError: {
        message: AuthErrorStrings.NETWORK_ERROR,
    },
    default: {
        message: AuthErrorStrings.DEFAULT_MSG,
    },
};

/*
 * Copyright 2017-2019 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"). You may not use this file except in compliance with
 * the License. A copy of the License is located at
 *
 *	 http://aws.amazon.com/apache2.0/
 *
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
 * and limitations under the License.
 */
var __assign = (undefined && undefined.__assign) || function () {
    __assign = Object.assign || function(t) {
        for (var s, i = 1, n = arguments.length; i < n; i++) {
            s = arguments[i];
            for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
                t[p] = s[p];
        }
        return t;
    };
    return __assign.apply(this, arguments);
};
var __awaiter = (undefined && undefined.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (undefined && undefined.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
var __read = (undefined && undefined.__read) || function (o, n) {
    var m = typeof Symbol === "function" && o[Symbol.iterator];
    if (!m) return o;
    var i = m.call(o), r, ar = [], e;
    try {
        while ((n === void 0 || n-- > 0) && !(r = i.next()).done) ar.push(r.value);
    }
    catch (error) { e = { error: error }; }
    finally {
        try {
            if (r && !r.done && (m = i["return"])) m.call(i);
        }
        finally { if (e) throw e.error; }
    }
    return ar;
};
var logger = new ConsoleLogger('AuthClass');
var USER_ADMIN_SCOPE = 'aws.cognito.signin.user.admin';
// 10 sec, following this guide https://www.nngroup.com/articles/response-times-3-important-limits/
var OAUTH_FLOW_MS_TIMEOUT = 10 * 1000;
var AMPLIFY_SYMBOL = (typeof Symbol !== 'undefined' && typeof Symbol.for === 'function'
    ? Symbol.for('amplify_default')
    : '@@amplify_default');
var dispatchAuthEvent = function (event, data, message) {
    Hub.dispatch('auth', { event: event, data: data, message: message }, 'Auth', AMPLIFY_SYMBOL);
};
// Cognito Documentation for max device
// tslint:disable-next-line:max-line-length
// https://docs.aws.amazon.com/cognito-user-identity-pools/latest/APIReference/API_ListDevices.html#API_ListDevices_RequestSyntax
var MAX_DEVICES = 60;
/**
 * Provide authentication steps
 */
var AuthClass = /** @class */ (function () {
    /**
     * Initialize Auth with AWS configurations
     * @param {Object} config - Configuration of the Auth
     */
    function AuthClass(config) {
        var _this = this;
        this.userPool = null;
        this.user = null;
        this.oAuthFlowInProgress = false;
        this.Credentials = Credentials;
        this.wrapRefreshSessionCallback = function (callback) {
            var wrapped = function (error, data) {
                if (data) {
                    dispatchAuthEvent('tokenRefresh', undefined, "New token retrieved");
                }
                else {
                    dispatchAuthEvent('tokenRefresh_failure', error, "Failed to retrieve new token");
                }
                return callback(error, data);
            };
            return wrapped;
        }; // prettier-ignore
        this.configure(config);
        this.currentCredentials = this.currentCredentials.bind(this);
        this.currentUserCredentials = this.currentUserCredentials.bind(this);
        Hub.listen('auth', function (_a) {
            var payload = _a.payload;
            var event = payload.event;
            switch (event) {
                case 'signIn':
                    _this._storage.setItem('amplify-signin-with-hostedUI', 'false');
                    break;
                case 'signOut':
                    _this._storage.removeItem('amplify-signin-with-hostedUI');
                    break;
                case 'cognitoHostedUI':
                    _this._storage.setItem('amplify-signin-with-hostedUI', 'true');
                    break;
            }
        });
    }
    AuthClass.prototype.getModuleName = function () {
        return 'Auth';
    };
    AuthClass.prototype.configure = function (config) {
        var _this = this;
        if (!config)
            return this._config || {};
        logger.debug('configure Auth');
        var conf = Object.assign({}, this._config, Parser$1.parseMobilehubConfig(config).Auth, config);
        this._config = conf;
        var _a = this._config, userPoolId = _a.userPoolId, userPoolWebClientId = _a.userPoolWebClientId, cookieStorage = _a.cookieStorage, oauth = _a.oauth, region = _a.region, identityPoolId = _a.identityPoolId, mandatorySignIn = _a.mandatorySignIn, refreshHandlers = _a.refreshHandlers, identityPoolRegion = _a.identityPoolRegion, clientMetadata = _a.clientMetadata, endpoint = _a.endpoint;
        if (!this._config.storage) {
            // backward compatability
            if (cookieStorage)
                this._storage = new CookieStorage(cookieStorage);
            else {
                this._storage = config.ssr
                    ? new UniversalStorage()
                    : new StorageHelper$1().getStorage();
            }
        }
        else {
            if (!this._isValidAuthStorage(this._config.storage)) {
                logger.error('The storage in the Auth config is not valid!');
                throw new Error('Empty storage object');
            }
            this._storage = this._config.storage;
        }
        this._storageSync = Promise.resolve();
        if (typeof this._storage['sync'] === 'function') {
            this._storageSync = this._storage['sync']();
        }
        if (userPoolId) {
            var userPoolData = {
                UserPoolId: userPoolId,
                ClientId: userPoolWebClientId,
                endpoint: endpoint,
            };
            userPoolData.Storage = this._storage;
            this.userPool = new CognitoUserPool(userPoolData, this.wrapRefreshSessionCallback);
        }
        this.Credentials.configure({
            mandatorySignIn: mandatorySignIn,
            region: identityPoolRegion || region,
            userPoolId: userPoolId,
            identityPoolId: identityPoolId,
            refreshHandlers: refreshHandlers,
            storage: this._storage,
        });
        // initialize cognitoauth client if hosted ui options provided
        // to keep backward compatibility:
        var cognitoHostedUIConfig = oauth
            ? isCognitoHostedOpts(this._config.oauth)
                ? oauth
                : oauth.awsCognito
            : undefined;
        if (cognitoHostedUIConfig) {
            var cognitoAuthParams = Object.assign({
                cognitoClientId: userPoolWebClientId,
                UserPoolId: userPoolId,
                domain: cognitoHostedUIConfig['domain'],
                scopes: cognitoHostedUIConfig['scope'],
                redirectSignIn: cognitoHostedUIConfig['redirectSignIn'],
                redirectSignOut: cognitoHostedUIConfig['redirectSignOut'],
                responseType: cognitoHostedUIConfig['responseType'],
                Storage: this._storage,
                urlOpener: cognitoHostedUIConfig['urlOpener'],
                clientMetadata: clientMetadata,
            }, cognitoHostedUIConfig['options']);
            this._oAuthHandler = new OAuth$1({
                scopes: cognitoAuthParams.scopes,
                config: cognitoAuthParams,
                cognitoClientId: cognitoAuthParams.cognitoClientId,
            });
            // **NOTE** - Remove this in a future major release as it is a breaking change
            // Prevents _handleAuthResponse from being called multiple times in Expo
            // See https://github.com/aws-amplify/amplify-js/issues/4388
            var usedResponseUrls_1 = {};
            urlListener(function (_a) {
                var url = _a.url;
                if (usedResponseUrls_1[url]) {
                    return;
                }
                usedResponseUrls_1[url] = true;
                _this._handleAuthResponse(url);
            });
        }
        dispatchAuthEvent('configured', null, "The Auth category has been configured successfully");
        return this._config;
    };
    /**
     * Sign up with username, password and other attributes like phone, email
     * @param {String | object} params - The user attributes used for signin
     * @param {String[]} restOfAttrs - for the backward compatability
     * @return - A promise resolves callback data if success
     */
    AuthClass.prototype.signUp = function (params) {
        var _this = this;
        var restOfAttrs = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            restOfAttrs[_i - 1] = arguments[_i];
        }
        if (!this.userPool) {
            return this.rejectNoUserPool();
        }
        var username = null;
        var password = null;
        var attributes = [];
        var validationData = null;
        var clientMetadata;
        if (params && typeof params === 'string') {
            username = params;
            password = restOfAttrs ? restOfAttrs[0] : null;
            var email = restOfAttrs ? restOfAttrs[1] : null;
            var phone_number = restOfAttrs ? restOfAttrs[2] : null;
            if (email)
                attributes.push(new CognitoUserAttribute({ Name: 'email', Value: email }));
            if (phone_number)
                attributes.push(new CognitoUserAttribute({
                    Name: 'phone_number',
                    Value: phone_number,
                }));
        }
        else if (params && typeof params === 'object') {
            username = params['username'];
            password = params['password'];
            if (params && params.clientMetadata) {
                clientMetadata = params.clientMetadata;
            }
            else if (this._config.clientMetadata) {
                clientMetadata = this._config.clientMetadata;
            }
            var attrs_1 = params['attributes'];
            if (attrs_1) {
                Object.keys(attrs_1).map(function (key) {
                    attributes.push(new CognitoUserAttribute({ Name: key, Value: attrs_1[key] }));
                });
            }
            var validationDataObject_1 = params['validationData'];
            if (validationDataObject_1) {
                validationData = [];
                Object.keys(validationDataObject_1).map(function (key) {
                    validationData.push(new CognitoUserAttribute({
                        Name: key,
                        Value: validationDataObject_1[key],
                    }));
                });
            }
        }
        else {
            return this.rejectAuthError(AuthErrorTypes.SignUpError);
        }
        if (!username) {
            return this.rejectAuthError(AuthErrorTypes.EmptyUsername);
        }
        if (!password) {
            return this.rejectAuthError(AuthErrorTypes.EmptyPassword);
        }
        logger.debug('signUp attrs:', attributes);
        logger.debug('signUp validation data:', validationData);
        return new Promise(function (resolve, reject) {
            _this.userPool.signUp(username, password, attributes, validationData, function (err, data) {
                if (err) {
                    dispatchAuthEvent('signUp_failure', err, username + " failed to signup");
                    reject(err);
                }
                else {
                    dispatchAuthEvent('signUp', data, username + " has signed up successfully");
                    resolve(data);
                }
            }, clientMetadata);
        });
    };
    /**
     * Send the verification code to confirm sign up
     * @param {String} username - The username to be confirmed
     * @param {String} code - The verification code
     * @param {ConfirmSignUpOptions} options - other options for confirm signup
     * @return - A promise resolves callback data if success
     */
    AuthClass.prototype.confirmSignUp = function (username, code, options) {
        if (!this.userPool) {
            return this.rejectNoUserPool();
        }
        if (!username) {
            return this.rejectAuthError(AuthErrorTypes.EmptyUsername);
        }
        if (!code) {
            return this.rejectAuthError(AuthErrorTypes.EmptyCode);
        }
        var user = this.createCognitoUser(username);
        var forceAliasCreation = options && typeof options.forceAliasCreation === 'boolean'
            ? options.forceAliasCreation
            : true;
        var clientMetadata;
        if (options && options.clientMetadata) {
            clientMetadata = options.clientMetadata;
        }
        else if (this._config.clientMetadata) {
            clientMetadata = this._config.clientMetadata;
        }
        return new Promise(function (resolve, reject) {
            user.confirmRegistration(code, forceAliasCreation, function (err, data) {
                if (err) {
                    reject(err);
                }
                else {
                    resolve(data);
                }
            }, clientMetadata);
        });
    };
    /**
     * Resend the verification code
     * @param {String} username - The username to be confirmed
     * @param {ClientMetadata} clientMetadata - Metadata to be passed to Cognito Lambda triggers
     * @return - A promise resolves code delivery details if successful
     */
    AuthClass.prototype.resendSignUp = function (username, clientMetadata) {
        if (clientMetadata === void 0) { clientMetadata = this._config.clientMetadata; }
        if (!this.userPool) {
            return this.rejectNoUserPool();
        }
        if (!username) {
            return this.rejectAuthError(AuthErrorTypes.EmptyUsername);
        }
        var user = this.createCognitoUser(username);
        return new Promise(function (resolve, reject) {
            user.resendConfirmationCode(function (err, data) {
                if (err) {
                    reject(err);
                }
                else {
                    resolve(data);
                }
            }, clientMetadata);
        });
    };
    /**
     * Sign in
     * @param {String | SignInOpts} usernameOrSignInOpts - The username to be signed in or the sign in options
     * @param {String} password - The password of the username
     * @return - A promise resolves the CognitoUser
     */
    AuthClass.prototype.signIn = function (usernameOrSignInOpts, pw, clientMetadata) {
        if (clientMetadata === void 0) { clientMetadata = this._config.clientMetadata; }
        if (!this.userPool) {
            return this.rejectNoUserPool();
        }
        var username = null;
        var password = null;
        var validationData = {};
        // for backward compatibility
        if (typeof usernameOrSignInOpts === 'string') {
            username = usernameOrSignInOpts;
            password = pw;
        }
        else if (isUsernamePasswordOpts(usernameOrSignInOpts)) {
            if (typeof pw !== 'undefined') {
                logger.warn('The password should be defined under the first parameter object!');
            }
            username = usernameOrSignInOpts.username;
            password = usernameOrSignInOpts.password;
            validationData = usernameOrSignInOpts.validationData;
        }
        else {
            return this.rejectAuthError(AuthErrorTypes.InvalidUsername);
        }
        if (!username) {
            return this.rejectAuthError(AuthErrorTypes.EmptyUsername);
        }
        var authDetails = new AuthenticationDetails({
            Username: username,
            Password: password,
            ValidationData: validationData,
            ClientMetadata: clientMetadata,
        });
        if (password) {
            return this.signInWithPassword(authDetails);
        }
        else {
            return this.signInWithoutPassword(authDetails);
        }
    };
    /**
     * Return an object with the authentication callbacks
     * @param {CognitoUser} user - the cognito user object
     * @param {} resolve - function called when resolving the current step
     * @param {} reject - function called when rejecting the current step
     * @return - an object with the callback methods for user authentication
     */
    AuthClass.prototype.authCallbacks = function (user, resolve, reject) {
        var _this = this;
        var that = this;
        return {
            onSuccess: function (session) { return __awaiter(_this, void 0, void 0, function () {
                var cred, e_1, currentUser, e_2;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            logger.debug(session);
                            delete user['challengeName'];
                            delete user['challengeParam'];
                            _a.label = 1;
                        case 1:
                            _a.trys.push([1, 4, 5, 9]);
                            return [4 /*yield*/, this.Credentials.clear()];
                        case 2:
                            _a.sent();
                            return [4 /*yield*/, this.Credentials.set(session, 'session')];
                        case 3:
                            cred = _a.sent();
                            logger.debug('succeed to get cognito credentials', cred);
                            return [3 /*break*/, 9];
                        case 4:
                            e_1 = _a.sent();
                            logger.debug('cannot get cognito credentials', e_1);
                            return [3 /*break*/, 9];
                        case 5:
                            _a.trys.push([5, 7, , 8]);
                            return [4 /*yield*/, this.currentUserPoolUser()];
                        case 6:
                            currentUser = _a.sent();
                            that.user = currentUser;
                            dispatchAuthEvent('signIn', currentUser, "A user " + user.getUsername() + " has been signed in");
                            resolve(currentUser);
                            return [3 /*break*/, 8];
                        case 7:
                            e_2 = _a.sent();
                            logger.error('Failed to get the signed in user', e_2);
                            reject(e_2);
                            return [3 /*break*/, 8];
                        case 8: return [7 /*endfinally*/];
                        case 9: return [2 /*return*/];
                    }
                });
            }); },
            onFailure: function (err) {
                logger.debug('signIn failure', err);
                dispatchAuthEvent('signIn_failure', err, user.getUsername() + " failed to signin");
                reject(err);
            },
            customChallenge: function (challengeParam) {
                logger.debug('signIn custom challenge answer required');
                user['challengeName'] = 'CUSTOM_CHALLENGE';
                user['challengeParam'] = challengeParam;
                resolve(user);
            },
            mfaRequired: function (challengeName, challengeParam) {
                logger.debug('signIn MFA required');
                user['challengeName'] = challengeName;
                user['challengeParam'] = challengeParam;
                resolve(user);
            },
            mfaSetup: function (challengeName, challengeParam) {
                logger.debug('signIn mfa setup', challengeName);
                user['challengeName'] = challengeName;
                user['challengeParam'] = challengeParam;
                resolve(user);
            },
            newPasswordRequired: function (userAttributes, requiredAttributes) {
                logger.debug('signIn new password');
                user['challengeName'] = 'NEW_PASSWORD_REQUIRED';
                user['challengeParam'] = {
                    userAttributes: userAttributes,
                    requiredAttributes: requiredAttributes,
                };
                resolve(user);
            },
            totpRequired: function (challengeName, challengeParam) {
                logger.debug('signIn totpRequired');
                user['challengeName'] = challengeName;
                user['challengeParam'] = challengeParam;
                resolve(user);
            },
            selectMFAType: function (challengeName, challengeParam) {
                logger.debug('signIn selectMFAType', challengeName);
                user['challengeName'] = challengeName;
                user['challengeParam'] = challengeParam;
                resolve(user);
            },
        };
    };
    /**
     * Sign in with a password
     * @private
     * @param {AuthenticationDetails} authDetails - the user sign in data
     * @return - A promise resolves the CognitoUser object if success or mfa required
     */
    AuthClass.prototype.signInWithPassword = function (authDetails) {
        var _this = this;
        if (this.pendingSignIn) {
            throw new Error('Pending sign-in attempt already in progress');
        }
        var user = this.createCognitoUser(authDetails.getUsername());
        this.pendingSignIn = new Promise(function (resolve, reject) {
            user.authenticateUser(authDetails, _this.authCallbacks(user, function (value) {
                _this.pendingSignIn = null;
                resolve(value);
            }, function (error) {
                _this.pendingSignIn = null;
                reject(error);
            }));
        });
        return this.pendingSignIn;
    };
    /**
     * Sign in without a password
     * @private
     * @param {AuthenticationDetails} authDetails - the user sign in data
     * @return - A promise resolves the CognitoUser object if success or mfa required
     */
    AuthClass.prototype.signInWithoutPassword = function (authDetails) {
        var _this = this;
        var user = this.createCognitoUser(authDetails.getUsername());
        user.setAuthenticationFlowType('CUSTOM_AUTH');
        return new Promise(function (resolve, reject) {
            user.initiateAuth(authDetails, _this.authCallbacks(user, resolve, reject));
        });
    };
    /**
     * This was previously used by an authenticated user to get MFAOptions,
     * but no longer returns a meaningful response. Refer to the documentation for
     * how to setup and use MFA: https://docs.amplify.aws/lib/auth/mfa/q/platform/js
     * @deprecated
     * @param {CognitoUser} user - the current user
     * @return - A promise resolves the current preferred mfa option if success
     */
    AuthClass.prototype.getMFAOptions = function (user) {
        return new Promise(function (res, rej) {
            user.getMFAOptions(function (err, mfaOptions) {
                if (err) {
                    logger.debug('get MFA Options failed', err);
                    rej(err);
                    return;
                }
                logger.debug('get MFA options success', mfaOptions);
                res(mfaOptions);
                return;
            });
        });
    };
    /**
     * get preferred mfa method
     * @param {CognitoUser} user - the current cognito user
     * @param {GetPreferredMFAOpts} params - options for getting the current user preferred MFA
     */
    AuthClass.prototype.getPreferredMFA = function (user, params) {
        var _this = this;
        var that = this;
        return new Promise(function (res, rej) {
            var clientMetadata = _this._config.clientMetadata; // TODO: verify behavior if this is override during signIn
            var bypassCache = params ? params.bypassCache : false;
            user.getUserData(function (err, data) { return __awaiter(_this, void 0, void 0, function () {
                var cleanUpError_1, mfaType;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            if (!err) return [3 /*break*/, 5];
                            logger.debug('getting preferred mfa failed', err);
                            if (!this.isSessionInvalid(err)) return [3 /*break*/, 4];
                            _a.label = 1;
                        case 1:
                            _a.trys.push([1, 3, , 4]);
                            return [4 /*yield*/, this.cleanUpInvalidSession(user)];
                        case 2:
                            _a.sent();
                            return [3 /*break*/, 4];
                        case 3:
                            cleanUpError_1 = _a.sent();
                            rej(new Error("Session is invalid due to: " + err.message + " and failed to clean up invalid session: " + cleanUpError_1.message));
                            return [2 /*return*/];
                        case 4:
                            rej(err);
                            return [2 /*return*/];
                        case 5:
                            mfaType = that._getMfaTypeFromUserData(data);
                            if (!mfaType) {
                                rej('invalid MFA Type');
                                return [2 /*return*/];
                            }
                            else {
                                res(mfaType);
                                return [2 /*return*/];
                            }
                    }
                });
            }); }, { bypassCache: bypassCache, clientMetadata: clientMetadata });
        });
    };
    AuthClass.prototype._getMfaTypeFromUserData = function (data) {
        var ret = null;
        var preferredMFA = data.PreferredMfaSetting;
        // if the user has used Auth.setPreferredMFA() to setup the mfa type
        // then the "PreferredMfaSetting" would exist in the response
        if (preferredMFA) {
            ret = preferredMFA;
        }
        else {
            // if mfaList exists but empty, then its noMFA
            var mfaList = data.UserMFASettingList;
            if (!mfaList) {
                // if SMS was enabled by using Auth.enableSMS(),
                // the response would contain MFAOptions
                // as for now Cognito only supports for SMS, so we will say it is 'SMS_MFA'
                // if it does not exist, then it should be NOMFA
                var MFAOptions = data.MFAOptions;
                if (MFAOptions) {
                    ret = 'SMS_MFA';
                }
                else {
                    ret = 'NOMFA';
                }
            }
            else if (mfaList.length === 0) {
                ret = 'NOMFA';
            }
            else {
                logger.debug('invalid case for getPreferredMFA', data);
            }
        }
        return ret;
    };
    AuthClass.prototype._getUserData = function (user, params) {
        var _this = this;
        return new Promise(function (res, rej) {
            user.getUserData(function (err, data) { return __awaiter(_this, void 0, void 0, function () {
                var cleanUpError_2;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            if (!err) return [3 /*break*/, 5];
                            logger.debug('getting user data failed', err);
                            if (!this.isSessionInvalid(err)) return [3 /*break*/, 4];
                            _a.label = 1;
                        case 1:
                            _a.trys.push([1, 3, , 4]);
                            return [4 /*yield*/, this.cleanUpInvalidSession(user)];
                        case 2:
                            _a.sent();
                            return [3 /*break*/, 4];
                        case 3:
                            cleanUpError_2 = _a.sent();
                            rej(new Error("Session is invalid due to: " + err.message + " and failed to clean up invalid session: " + cleanUpError_2.message));
                            return [2 /*return*/];
                        case 4:
                            rej(err);
                            return [2 /*return*/];
                        case 5:
                            res(data);
                            _a.label = 6;
                        case 6: return [2 /*return*/];
                    }
                });
            }); }, params);
        });
    };
    /**
     * set preferred MFA method
     * @param {CognitoUser} user - the current Cognito user
     * @param {string} mfaMethod - preferred mfa method
     * @return - A promise resolve if success
     */
    AuthClass.prototype.setPreferredMFA = function (user, mfaMethod) {
        return __awaiter(this, void 0, void 0, function () {
            var clientMetadata, userData, smsMfaSettings, totpMfaSettings, _a, mfaList, currentMFAType;
            var _this = this;
            return __generator(this, function (_b) {
                switch (_b.label) {
                    case 0:
                        clientMetadata = this._config.clientMetadata;
                        return [4 /*yield*/, this._getUserData(user, {
                                bypassCache: true,
                                clientMetadata: clientMetadata,
                            })];
                    case 1:
                        userData = _b.sent();
                        smsMfaSettings = null;
                        totpMfaSettings = null;
                        _a = mfaMethod;
                        switch (_a) {
                            case 'TOTP': return [3 /*break*/, 2];
                            case 'SOFTWARE_TOKEN_MFA': return [3 /*break*/, 2];
                            case 'SMS': return [3 /*break*/, 3];
                            case 'SMS_MFA': return [3 /*break*/, 3];
                            case 'NOMFA': return [3 /*break*/, 4];
                        }
                        return [3 /*break*/, 6];
                    case 2:
                        totpMfaSettings = {
                            PreferredMfa: true,
                            Enabled: true,
                        };
                        return [3 /*break*/, 7];
                    case 3:
                        smsMfaSettings = {
                            PreferredMfa: true,
                            Enabled: true,
                        };
                        return [3 /*break*/, 7];
                    case 4:
                        mfaList = userData['UserMFASettingList'];
                        return [4 /*yield*/, this._getMfaTypeFromUserData(userData)];
                    case 5:
                        currentMFAType = _b.sent();
                        if (currentMFAType === 'NOMFA') {
                            return [2 /*return*/, Promise.resolve('No change for mfa type')];
                        }
                        else if (currentMFAType === 'SMS_MFA') {
                            smsMfaSettings = {
                                PreferredMfa: false,
                                Enabled: false,
                            };
                        }
                        else if (currentMFAType === 'SOFTWARE_TOKEN_MFA') {
                            totpMfaSettings = {
                                PreferredMfa: false,
                                Enabled: false,
                            };
                        }
                        else {
                            return [2 /*return*/, this.rejectAuthError(AuthErrorTypes.InvalidMFA)];
                        }
                        // if there is a UserMFASettingList in the response
                        // we need to disable every mfa type in that list
                        if (mfaList && mfaList.length !== 0) {
                            // to disable SMS or TOTP if exists in that list
                            mfaList.forEach(function (mfaType) {
                                if (mfaType === 'SMS_MFA') {
                                    smsMfaSettings = {
                                        PreferredMfa: false,
                                        Enabled: false,
                                    };
                                }
                                else if (mfaType === 'SOFTWARE_TOKEN_MFA') {
                                    totpMfaSettings = {
                                        PreferredMfa: false,
                                        Enabled: false,
                                    };
                                }
                            });
                        }
                        return [3 /*break*/, 7];
                    case 6:
                        logger.debug('no validmfa method provided');
                        return [2 /*return*/, this.rejectAuthError(AuthErrorTypes.NoMFA)];
                    case 7:
                        return [2 /*return*/, new Promise(function (res, rej) {
                                user.setUserMfaPreference(smsMfaSettings, totpMfaSettings, function (err, result) {
                                    if (err) {
                                        logger.debug('Set user mfa preference error', err);
                                        return rej(err);
                                    }
                                    logger.debug('Set user mfa success', result);
                                    logger.debug('Caching the latest user data into local');
                                    // cache the latest result into user data
                                    user.getUserData(function (err, data) { return __awaiter(_this, void 0, void 0, function () {
                                        var cleanUpError_3;
                                        return __generator(this, function (_a) {
                                            switch (_a.label) {
                                                case 0:
                                                    if (!err) return [3 /*break*/, 5];
                                                    logger.debug('getting user data failed', err);
                                                    if (!this.isSessionInvalid(err)) return [3 /*break*/, 4];
                                                    _a.label = 1;
                                                case 1:
                                                    _a.trys.push([1, 3, , 4]);
                                                    return [4 /*yield*/, this.cleanUpInvalidSession(user)];
                                                case 2:
                                                    _a.sent();
                                                    return [3 /*break*/, 4];
                                                case 3:
                                                    cleanUpError_3 = _a.sent();
                                                    rej(new Error("Session is invalid due to: " + err.message + " and failed to clean up invalid session: " + cleanUpError_3.message));
                                                    return [2 /*return*/];
                                                case 4: return [2 /*return*/, rej(err)];
                                                case 5: return [2 /*return*/, res(result)];
                                            }
                                        });
                                    }); }, {
                                        bypassCache: true,
                                        clientMetadata: clientMetadata,
                                    });
                                });
                            })];
                }
            });
        });
    };
    /**
     * disable SMS
     * @deprecated
     * @param {CognitoUser} user - the current user
     * @return - A promise resolves is success
     */
    AuthClass.prototype.disableSMS = function (user) {
        return new Promise(function (res, rej) {
            user.disableMFA(function (err, data) {
                if (err) {
                    logger.debug('disable mfa failed', err);
                    rej(err);
                    return;
                }
                logger.debug('disable mfa succeed', data);
                res(data);
                return;
            });
        });
    };
    /**
     * enable SMS
     * @deprecated
     * @param {CognitoUser} user - the current user
     * @return - A promise resolves is success
     */
    AuthClass.prototype.enableSMS = function (user) {
        return new Promise(function (res, rej) {
            user.enableMFA(function (err, data) {
                if (err) {
                    logger.debug('enable mfa failed', err);
                    rej(err);
                    return;
                }
                logger.debug('enable mfa succeed', data);
                res(data);
                return;
            });
        });
    };
    /**
     * Setup TOTP
     * @param {CognitoUser} user - the current user
     * @return - A promise resolves with the secret code if success
     */
    AuthClass.prototype.setupTOTP = function (user) {
        return new Promise(function (res, rej) {
            user.associateSoftwareToken({
                onFailure: function (err) {
                    logger.debug('associateSoftwareToken failed', err);
                    rej(err);
                    return;
                },
                associateSecretCode: function (secretCode) {
                    logger.debug('associateSoftwareToken sucess', secretCode);
                    res(secretCode);
                    return;
                },
            });
        });
    };
    /**
     * verify TOTP setup
     * @param {CognitoUser} user - the current user
     * @param {string} challengeAnswer - challenge answer
     * @return - A promise resolves is success
     */
    AuthClass.prototype.verifyTotpToken = function (user, challengeAnswer) {
        logger.debug('verification totp token', user, challengeAnswer);
        return new Promise(function (res, rej) {
            user.verifySoftwareToken(challengeAnswer, 'My TOTP device', {
                onFailure: function (err) {
                    logger.debug('verifyTotpToken failed', err);
                    rej(err);
                    return;
                },
                onSuccess: function (data) {
                    dispatchAuthEvent('signIn', user, "A user " + user.getUsername() + " has been signed in");
                    logger.debug('verifyTotpToken success', data);
                    res(data);
                    return;
                },
            });
        });
    };
    /**
     * Send MFA code to confirm sign in
     * @param {Object} user - The CognitoUser object
     * @param {String} code - The confirmation code
     */
    AuthClass.prototype.confirmSignIn = function (user, code, mfaType, clientMetadata) {
        var _this = this;
        if (clientMetadata === void 0) { clientMetadata = this._config.clientMetadata; }
        if (!code) {
            return this.rejectAuthError(AuthErrorTypes.EmptyCode);
        }
        var that = this;
        return new Promise(function (resolve, reject) {
            user.sendMFACode(code, {
                onSuccess: function (session) { return __awaiter(_this, void 0, void 0, function () {
                    var cred, e_3;
                    return __generator(this, function (_a) {
                        switch (_a.label) {
                            case 0:
                                logger.debug(session);
                                _a.label = 1;
                            case 1:
                                _a.trys.push([1, 4, 5, 6]);
                                return [4 /*yield*/, this.Credentials.clear()];
                            case 2:
                                _a.sent();
                                return [4 /*yield*/, this.Credentials.set(session, 'session')];
                            case 3:
                                cred = _a.sent();
                                logger.debug('succeed to get cognito credentials', cred);
                                return [3 /*break*/, 6];
                            case 4:
                                e_3 = _a.sent();
                                logger.debug('cannot get cognito credentials', e_3);
                                return [3 /*break*/, 6];
                            case 5:
                                that.user = user;
                                dispatchAuthEvent('signIn', user, "A user " + user.getUsername() + " has been signed in");
                                resolve(user);
                                return [7 /*endfinally*/];
                            case 6: return [2 /*return*/];
                        }
                    });
                }); },
                onFailure: function (err) {
                    logger.debug('confirm signIn failure', err);
                    reject(err);
                },
            }, mfaType, clientMetadata);
        });
    };
    AuthClass.prototype.completeNewPassword = function (user, password, requiredAttributes, clientMetadata) {
        var _this = this;
        if (requiredAttributes === void 0) { requiredAttributes = {}; }
        if (clientMetadata === void 0) { clientMetadata = this._config.clientMetadata; }
        if (!password) {
            return this.rejectAuthError(AuthErrorTypes.EmptyPassword);
        }
        var that = this;
        return new Promise(function (resolve, reject) {
            user.completeNewPasswordChallenge(password, requiredAttributes, {
                onSuccess: function (session) { return __awaiter(_this, void 0, void 0, function () {
                    var cred, e_4;
                    return __generator(this, function (_a) {
                        switch (_a.label) {
                            case 0:
                                logger.debug(session);
                                _a.label = 1;
                            case 1:
                                _a.trys.push([1, 4, 5, 6]);
                                return [4 /*yield*/, this.Credentials.clear()];
                            case 2:
                                _a.sent();
                                return [4 /*yield*/, this.Credentials.set(session, 'session')];
                            case 3:
                                cred = _a.sent();
                                logger.debug('succeed to get cognito credentials', cred);
                                return [3 /*break*/, 6];
                            case 4:
                                e_4 = _a.sent();
                                logger.debug('cannot get cognito credentials', e_4);
                                return [3 /*break*/, 6];
                            case 5:
                                that.user = user;
                                dispatchAuthEvent('signIn', user, "A user " + user.getUsername() + " has been signed in");
                                resolve(user);
                                return [7 /*endfinally*/];
                            case 6: return [2 /*return*/];
                        }
                    });
                }); },
                onFailure: function (err) {
                    logger.debug('completeNewPassword failure', err);
                    dispatchAuthEvent('completeNewPassword_failure', err, _this.user + " failed to complete the new password flow");
                    reject(err);
                },
                mfaRequired: function (challengeName, challengeParam) {
                    logger.debug('signIn MFA required');
                    user['challengeName'] = challengeName;
                    user['challengeParam'] = challengeParam;
                    resolve(user);
                },
                mfaSetup: function (challengeName, challengeParam) {
                    logger.debug('signIn mfa setup', challengeName);
                    user['challengeName'] = challengeName;
                    user['challengeParam'] = challengeParam;
                    resolve(user);
                },
                totpRequired: function (challengeName, challengeParam) {
                    logger.debug('signIn mfa setup', challengeName);
                    user['challengeName'] = challengeName;
                    user['challengeParam'] = challengeParam;
                    resolve(user);
                },
            }, clientMetadata);
        });
    };
    /**
     * Send the answer to a custom challenge
     * @param {CognitoUser} user - The CognitoUser object
     * @param {String} challengeResponses - The confirmation code
     */
    AuthClass.prototype.sendCustomChallengeAnswer = function (user, challengeResponses, clientMetadata) {
        var _this = this;
        if (clientMetadata === void 0) { clientMetadata = this._config.clientMetadata; }
        if (!this.userPool) {
            return this.rejectNoUserPool();
        }
        if (!challengeResponses) {
            return this.rejectAuthError(AuthErrorTypes.EmptyChallengeResponse);
        }
        return new Promise(function (resolve, reject) {
            user.sendCustomChallengeAnswer(challengeResponses, _this.authCallbacks(user, resolve, reject), clientMetadata);
        });
    };
    /**
     * Delete an authenticated users' attributes
     * @param {CognitoUser} - The currently logged in user object
     * @return {Promise}
     **/
    AuthClass.prototype.deleteUserAttributes = function (user, attributeNames) {
        var that = this;
        return new Promise(function (resolve, reject) {
            that.userSession(user).then(function (session) {
                user.deleteAttributes(attributeNames, function (err, result) {
                    if (err) {
                        return reject(err);
                    }
                    else {
                        return resolve(result);
                    }
                });
            });
        });
    };
    /**
     * Delete the current authenticated user
     * @return {Promise}
     **/
    // TODO: Check return type void
    AuthClass.prototype.deleteUser = function () {
        return __awaiter(this, void 0, void 0, function () {
            var e_5, isSignedInHostedUI;
            var _this = this;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        _a.trys.push([0, 2, , 3]);
                        return [4 /*yield*/, this._storageSync];
                    case 1:
                        _a.sent();
                        return [3 /*break*/, 3];
                    case 2:
                        e_5 = _a.sent();
                        logger.debug('Failed to sync cache info into memory', e_5);
                        throw new Error(e_5);
                    case 3:
                        isSignedInHostedUI = this._oAuthHandler &&
                            this._storage.getItem('amplify-signin-with-hostedUI') === 'true';
                        return [2 /*return*/, new Promise(function (res, rej) { return __awaiter(_this, void 0, void 0, function () {
                                var user_1;
                                var _this = this;
                                return __generator(this, function (_a) {
                                    if (this.userPool) {
                                        user_1 = this.userPool.getCurrentUser();
                                        if (!user_1) {
                                            logger.debug('Failed to get user from user pool');
                                            return [2 /*return*/, rej(new Error('No current user.'))];
                                        }
                                        else {
                                            user_1.getSession(function (err, session) { return __awaiter(_this, void 0, void 0, function () {
                                                var cleanUpError_4;
                                                var _this = this;
                                                return __generator(this, function (_a) {
                                                    switch (_a.label) {
                                                        case 0:
                                                            if (!err) return [3 /*break*/, 5];
                                                            logger.debug('Failed to get the user session', err);
                                                            if (!this.isSessionInvalid(err)) return [3 /*break*/, 4];
                                                            _a.label = 1;
                                                        case 1:
                                                            _a.trys.push([1, 3, , 4]);
                                                            return [4 /*yield*/, this.cleanUpInvalidSession(user_1)];
                                                        case 2:
                                                            _a.sent();
                                                            return [3 /*break*/, 4];
                                                        case 3:
                                                            cleanUpError_4 = _a.sent();
                                                            rej(new Error("Session is invalid due to: " + err.message + " and failed to clean up invalid session: " + cleanUpError_4.message));
                                                            return [2 /*return*/];
                                                        case 4: return [2 /*return*/, rej(err)];
                                                        case 5:
                                                            user_1.deleteUser(function (err, result) {
                                                                if (err) {
                                                                    rej(err);
                                                                }
                                                                else {
                                                                    dispatchAuthEvent('userDeleted', result, 'The authenticated user has been deleted.');
                                                                    user_1.signOut();
                                                                    _this.user = null;
                                                                    try {
                                                                        _this.cleanCachedItems(); // clean aws credentials
                                                                    }
                                                                    catch (e) {
                                                                        // TODO: change to rejects in refactor
                                                                        logger.debug('failed to clear cached items');
                                                                    }
                                                                    if (isSignedInHostedUI) {
                                                                        _this.oAuthSignOutRedirect(res, rej);
                                                                    }
                                                                    else {
                                                                        dispatchAuthEvent('signOut', _this.user, "A user has been signed out");
                                                                        res(result);
                                                                    }
                                                                }
                                                            });
                                                            _a.label = 6;
                                                        case 6: return [2 /*return*/];
                                                    }
                                                });
                                            }); });
                                        }
                                    }
                                    else {
                                        logger.debug('no Congito User pool');
                                        rej(new Error('Cognito User pool does not exist'));
                                    }
                                    return [2 /*return*/];
                                });
                            }); })];
                }
            });
        });
    };
    /**
     * Update an authenticated users' attributes
     * @param {CognitoUser} - The currently logged in user object
     * @return {Promise}
     **/
    AuthClass.prototype.updateUserAttributes = function (user, attributes, clientMetadata) {
        if (clientMetadata === void 0) { clientMetadata = this._config.clientMetadata; }
        var attributeList = [];
        var that = this;
        return new Promise(function (resolve, reject) {
            that.userSession(user).then(function (session) {
                for (var key in attributes) {
                    if (key !== 'sub' && key.indexOf('_verified') < 0) {
                        var attr = {
                            Name: key,
                            Value: attributes[key],
                        };
                        attributeList.push(attr);
                    }
                }
                user.updateAttributes(attributeList, function (err, result) {
                    if (err) {
                        return reject(err);
                    }
                    else {
                        return resolve(result);
                    }
                }, clientMetadata);
            });
        });
    };
    /**
     * Return user attributes
     * @param {Object} user - The CognitoUser object
     * @return - A promise resolves to user attributes if success
     */
    AuthClass.prototype.userAttributes = function (user) {
        var _this = this;
        return new Promise(function (resolve, reject) {
            _this.userSession(user).then(function (session) {
                user.getUserAttributes(function (err, attributes) {
                    if (err) {
                        reject(err);
                    }
                    else {
                        resolve(attributes);
                    }
                });
            });
        });
    };
    AuthClass.prototype.verifiedContact = function (user) {
        var that = this;
        return this.userAttributes(user).then(function (attributes) {
            var attrs = that.attributesToObject(attributes);
            var unverified = {};
            var verified = {};
            if (attrs['email']) {
                if (attrs['email_verified']) {
                    verified['email'] = attrs['email'];
                }
                else {
                    unverified['email'] = attrs['email'];
                }
            }
            if (attrs['phone_number']) {
                if (attrs['phone_number_verified']) {
                    verified['phone_number'] = attrs['phone_number'];
                }
                else {
                    unverified['phone_number'] = attrs['phone_number'];
                }
            }
            return {
                verified: verified,
                unverified: unverified,
            };
        });
    };
    AuthClass.prototype.isErrorWithMessage = function (err) {
        return (typeof err === 'object' &&
            Object.prototype.hasOwnProperty.call(err, 'message'));
    };
    // Session revoked by another app
    AuthClass.prototype.isTokenRevokedError = function (err) {
        return (this.isErrorWithMessage(err) &&
            err.message === 'Access Token has been revoked');
    };
    AuthClass.prototype.isRefreshTokenRevokedError = function (err) {
        return (this.isErrorWithMessage(err) &&
            err.message === 'Refresh Token has been revoked');
    };
    AuthClass.prototype.isUserDisabledError = function (err) {
        return this.isErrorWithMessage(err) && err.message === 'User is disabled.';
    };
    AuthClass.prototype.isUserDoesNotExistError = function (err) {
        return (this.isErrorWithMessage(err) && err.message === 'User does not exist.');
    };
    AuthClass.prototype.isRefreshTokenExpiredError = function (err) {
        return (this.isErrorWithMessage(err) &&
            err.message === 'Refresh Token has expired');
    };
    AuthClass.prototype.isSignedInHostedUI = function () {
        return (this._oAuthHandler &&
            this._storage.getItem('amplify-signin-with-hostedUI') === 'true');
    };
    AuthClass.prototype.isSessionInvalid = function (err) {
        return (this.isUserDisabledError(err) ||
            this.isUserDoesNotExistError(err) ||
            this.isTokenRevokedError(err) ||
            this.isRefreshTokenRevokedError(err) ||
            this.isRefreshTokenExpiredError(err));
    };
    AuthClass.prototype.cleanUpInvalidSession = function (user) {
        return __awaiter(this, void 0, void 0, function () {
            var _this = this;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        user.signOut();
                        this.user = null;
                        _a.label = 1;
                    case 1:
                        _a.trys.push([1, 3, , 4]);
                        return [4 /*yield*/, this.cleanCachedItems()];
                    case 2:
                        _a.sent(); // clean aws credentials
                        return [3 /*break*/, 4];
                    case 3:
                        _a.sent();
                        logger.debug('failed to clear cached items');
                        return [3 /*break*/, 4];
                    case 4:
                        if (this.isSignedInHostedUI()) {
                            return [2 /*return*/, new Promise(function (res, rej) {
                                    _this.oAuthSignOutRedirect(res, rej);
                                })];
                        }
                        else {
                            dispatchAuthEvent('signOut', this.user, "A user has been signed out");
                        }
                        return [2 /*return*/];
                }
            });
        });
    };
    /**
     * Get current authenticated user
     * @return - A promise resolves to current authenticated CognitoUser if success
     */
    AuthClass.prototype.currentUserPoolUser = function (params) {
        var _this = this;
        if (!this.userPool) {
            return this.rejectNoUserPool();
        }
        return new Promise(function (res, rej) {
            _this._storageSync
                .then(function () { return __awaiter(_this, void 0, void 0, function () {
                var user, clientMetadata;
                var _this = this;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            if (!this.isOAuthInProgress()) return [3 /*break*/, 2];
                            logger.debug('OAuth signIn in progress, waiting for resolution...');
                            return [4 /*yield*/, new Promise(function (res) {
                                    var timeoutId = setTimeout(function () {
                                        logger.debug('OAuth signIn in progress timeout');
                                        Hub.remove('auth', hostedUISignCallback);
                                        res();
                                    }, OAUTH_FLOW_MS_TIMEOUT);
                                    Hub.listen('auth', hostedUISignCallback);
                                    function hostedUISignCallback(_a) {
                                        var payload = _a.payload;
                                        var event = payload.event;
                                        if (event === 'cognitoHostedUI' ||
                                            event === 'cognitoHostedUI_failure') {
                                            logger.debug("OAuth signIn resolved: " + event);
                                            clearTimeout(timeoutId);
                                            Hub.remove('auth', hostedUISignCallback);
                                            res();
                                        }
                                    }
                                })];
                        case 1:
                            _a.sent();
                            _a.label = 2;
                        case 2:
                            user = this.userPool.getCurrentUser();
                            if (!user) {
                                logger.debug('Failed to get user from user pool');
                                rej('No current user');
                                return [2 /*return*/];
                            }
                            clientMetadata = this._config.clientMetadata;
                            // refresh the session if the session expired.
                            user.getSession(function (err, session) { return __awaiter(_this, void 0, void 0, function () {
                                var cleanUpError_5, bypassCache, clientMetadata, _a, scope;
                                var _this = this;
                                return __generator(this, function (_b) {
                                    switch (_b.label) {
                                        case 0:
                                            if (!err) return [3 /*break*/, 5];
                                            logger.debug('Failed to get the user session', err);
                                            if (!this.isSessionInvalid(err)) return [3 /*break*/, 4];
                                            _b.label = 1;
                                        case 1:
                                            _b.trys.push([1, 3, , 4]);
                                            return [4 /*yield*/, this.cleanUpInvalidSession(user)];
                                        case 2:
                                            _b.sent();
                                            return [3 /*break*/, 4];
                                        case 3:
                                            cleanUpError_5 = _b.sent();
                                            rej(new Error("Session is invalid due to: " + err.message + " and failed to clean up invalid session: " + cleanUpError_5.message));
                                            return [2 /*return*/];
                                        case 4:
                                            rej(err);
                                            return [2 /*return*/];
                                        case 5:
                                            bypassCache = params ? params.bypassCache : false;
                                            if (!bypassCache) return [3 /*break*/, 7];
                                            return [4 /*yield*/, this.Credentials.clear()];
                                        case 6:
                                            _b.sent();
                                            _b.label = 7;
                                        case 7:
                                            clientMetadata = this._config.clientMetadata;
                                            _a = session.getAccessToken().decodePayload().scope, scope = _a === void 0 ? '' : _a;
                                            if (scope.split(' ').includes(USER_ADMIN_SCOPE)) {
                                                user.getUserData(function (err, data) { return __awaiter(_this, void 0, void 0, function () {
                                                    var cleanUpError_6, preferredMFA, attributeList, i, attribute, userAttribute, attributes;
                                                    return __generator(this, function (_a) {
                                                        switch (_a.label) {
                                                            case 0:
                                                                if (!err) return [3 /*break*/, 7];
                                                                logger.debug('getting user data failed', err);
                                                                if (!this.isSessionInvalid(err)) return [3 /*break*/, 5];
                                                                _a.label = 1;
                                                            case 1:
                                                                _a.trys.push([1, 3, , 4]);
                                                                return [4 /*yield*/, this.cleanUpInvalidSession(user)];
                                                            case 2:
                                                                _a.sent();
                                                                return [3 /*break*/, 4];
                                                            case 3:
                                                                cleanUpError_6 = _a.sent();
                                                                rej(new Error("Session is invalid due to: " + err.message + " and failed to clean up invalid session: " + cleanUpError_6.message));
                                                                return [2 /*return*/];
                                                            case 4:
                                                                rej(err);
                                                                return [3 /*break*/, 6];
                                                            case 5:
                                                                res(user);
                                                                _a.label = 6;
                                                            case 6: return [2 /*return*/];
                                                            case 7:
                                                                preferredMFA = data.PreferredMfaSetting || 'NOMFA';
                                                                attributeList = [];
                                                                for (i = 0; i < data.UserAttributes.length; i++) {
                                                                    attribute = {
                                                                        Name: data.UserAttributes[i].Name,
                                                                        Value: data.UserAttributes[i].Value,
                                                                    };
                                                                    userAttribute = new CognitoUserAttribute(attribute);
                                                                    attributeList.push(userAttribute);
                                                                }
                                                                attributes = this.attributesToObject(attributeList);
                                                                Object.assign(user, { attributes: attributes, preferredMFA: preferredMFA });
                                                                return [2 /*return*/, res(user)];
                                                        }
                                                    });
                                                }); }, { bypassCache: bypassCache, clientMetadata: clientMetadata });
                                            }
                                            else {
                                                logger.debug("Unable to get the user data because the " + USER_ADMIN_SCOPE + " " +
                                                    "is not in the scopes of the access token");
                                                return [2 /*return*/, res(user)];
                                            }
                                            return [2 /*return*/];
                                    }
                                });
                            }); }, { clientMetadata: clientMetadata });
                            return [2 /*return*/];
                    }
                });
            }); })
                .catch(function (e) {
                logger.debug('Failed to sync cache info into memory', e);
                return rej(e);
            });
        });
    };
    AuthClass.prototype.isOAuthInProgress = function () {
        return this.oAuthFlowInProgress;
    };
    /**
     * Get current authenticated user
     * @param {CurrentUserOpts} - options for getting the current user
     * @return - A promise resolves to current authenticated CognitoUser if success
     */
    AuthClass.prototype.currentAuthenticatedUser = function (params) {
        return __awaiter(this, void 0, void 0, function () {
            var federatedUser, e_7, federatedInfo, user, e_8;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        logger.debug('getting current authenticated user');
                        federatedUser = null;
                        _a.label = 1;
                    case 1:
                        _a.trys.push([1, 3, , 4]);
                        return [4 /*yield*/, this._storageSync];
                    case 2:
                        _a.sent();
                        return [3 /*break*/, 4];
                    case 3:
                        e_7 = _a.sent();
                        logger.debug('Failed to sync cache info into memory', e_7);
                        throw e_7;
                    case 4:
                        try {
                            federatedInfo = JSON.parse(this._storage.getItem('aws-amplify-federatedInfo'));
                            if (federatedInfo) {
                                federatedUser = __assign(__assign({}, federatedInfo.user), { token: federatedInfo.token });
                            }
                        }
                        catch (e) {
                            logger.debug('cannot load federated user from auth storage');
                        }
                        if (!federatedUser) return [3 /*break*/, 5];
                        this.user = federatedUser;
                        logger.debug('get current authenticated federated user', this.user);
                        return [2 /*return*/, this.user];
                    case 5:
                        logger.debug('get current authenticated userpool user');
                        user = null;
                        _a.label = 6;
                    case 6:
                        _a.trys.push([6, 8, , 9]);
                        return [4 /*yield*/, this.currentUserPoolUser(params)];
                    case 7:
                        user = _a.sent();
                        return [3 /*break*/, 9];
                    case 8:
                        e_8 = _a.sent();
                        if (e_8 === 'No userPool') {
                            logger.error('Cannot get the current user because the user pool is missing. ' +
                                'Please make sure the Auth module is configured with a valid Cognito User Pool ID');
                        }
                        logger.debug('The user is not authenticated by the error', e_8);
                        return [2 /*return*/, Promise.reject('The user is not authenticated')];
                    case 9:
                        this.user = user;
                        return [2 /*return*/, this.user];
                }
            });
        });
    };
    /**
     * Get current user's session
     * @return - A promise resolves to session object if success
     */
    AuthClass.prototype.currentSession = function () {
        var that = this;
        logger.debug('Getting current session');
        // Purposely not calling the reject method here because we don't need a console error
        if (!this.userPool) {
            return this.rejectNoUserPool();
        }
        return new Promise(function (res, rej) {
            that
                .currentUserPoolUser()
                .then(function (user) {
                that
                    .userSession(user)
                    .then(function (session) {
                    res(session);
                    return;
                })
                    .catch(function (e) {
                    logger.debug('Failed to get the current session', e);
                    rej(e);
                    return;
                });
            })
                .catch(function (e) {
                logger.debug('Failed to get the current user', e);
                rej(e);
                return;
            });
        });
    };
    /**
     * Get the corresponding user session
     * @param {Object} user - The CognitoUser object
     * @return - A promise resolves to the session
     */
    AuthClass.prototype.userSession = function (user) {
        var _this = this;
        if (!user) {
            logger.debug('the user is null');
            return this.rejectAuthError(AuthErrorTypes.NoUserSession);
        }
        var clientMetadata = this._config.clientMetadata; // TODO: verify behavior if this is override during signIn
        return new Promise(function (res, rej) {
            logger.debug('Getting the session from this user:', user);
            user.getSession(function (err, session) { return __awaiter(_this, void 0, void 0, function () {
                var cleanUpError_7;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            if (!err) return [3 /*break*/, 5];
                            logger.debug('Failed to get the session from user', user);
                            if (!this.isSessionInvalid(err)) return [3 /*break*/, 4];
                            _a.label = 1;
                        case 1:
                            _a.trys.push([1, 3, , 4]);
                            return [4 /*yield*/, this.cleanUpInvalidSession(user)];
                        case 2:
                            _a.sent();
                            return [3 /*break*/, 4];
                        case 3:
                            cleanUpError_7 = _a.sent();
                            rej(new Error("Session is invalid due to: " + err.message + " and failed to clean up invalid session: " + cleanUpError_7.message));
                            return [2 /*return*/];
                        case 4:
                            rej(err);
                            return [2 /*return*/];
                        case 5:
                            logger.debug('Succeed to get the user session', session);
                            res(session);
                            return [2 /*return*/];
                    }
                });
            }); }, { clientMetadata: clientMetadata });
        });
    };
    /**
     * Get authenticated credentials of current user.
     * @return - A promise resolves to be current user's credentials
     */
    AuthClass.prototype.currentUserCredentials = function () {
        return __awaiter(this, void 0, void 0, function () {
            var e_9, federatedInfo;
            var _this = this;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        logger.debug('Getting current user credentials');
                        _a.label = 1;
                    case 1:
                        _a.trys.push([1, 3, , 4]);
                        return [4 /*yield*/, this._storageSync];
                    case 2:
                        _a.sent();
                        return [3 /*break*/, 4];
                    case 3:
                        e_9 = _a.sent();
                        logger.debug('Failed to sync cache info into memory', e_9);
                        throw e_9;
                    case 4:
                        federatedInfo = null;
                        try {
                            federatedInfo = JSON.parse(this._storage.getItem('aws-amplify-federatedInfo'));
                        }
                        catch (e) {
                            logger.debug('failed to get or parse item aws-amplify-federatedInfo', e);
                        }
                        if (federatedInfo) {
                            // refresh the jwt token here if necessary
                            return [2 /*return*/, this.Credentials.refreshFederatedToken(federatedInfo)];
                        }
                        else {
                            return [2 /*return*/, this.currentSession()
                                    .then(function (session) {
                                    logger.debug('getting session success', session);
                                    return _this.Credentials.set(session, 'session');
                                })
                                    .catch(function () {
                                    logger.debug('getting guest credentials');
                                    return _this.Credentials.set(null, 'guest');
                                })];
                        }
                }
            });
        });
    };
    AuthClass.prototype.currentCredentials = function () {
        logger.debug('getting current credentials');
        return this.Credentials.get();
    };
    /**
     * Initiate an attribute confirmation request
     * @param {Object} user - The CognitoUser
     * @param {Object} attr - The attributes to be verified
     * @return - A promise resolves to callback data if success
     */
    AuthClass.prototype.verifyUserAttribute = function (user, attr, clientMetadata) {
        if (clientMetadata === void 0) { clientMetadata = this._config.clientMetadata; }
        return new Promise(function (resolve, reject) {
            user.getAttributeVerificationCode(attr, {
                onSuccess: function (success) {
                    return resolve(success);
                },
                onFailure: function (err) {
                    return reject(err);
                },
            }, clientMetadata);
        });
    };
    /**
     * Confirm an attribute using a confirmation code
     * @param {Object} user - The CognitoUser
     * @param {Object} attr - The attribute to be verified
     * @param {String} code - The confirmation code
     * @return - A promise resolves to callback data if success
     */
    AuthClass.prototype.verifyUserAttributeSubmit = function (user, attr, code) {
        if (!code) {
            return this.rejectAuthError(AuthErrorTypes.EmptyCode);
        }
        return new Promise(function (resolve, reject) {
            user.verifyAttribute(attr, code, {
                onSuccess: function (data) {
                    resolve(data);
                    return;
                },
                onFailure: function (err) {
                    reject(err);
                    return;
                },
            });
        });
    };
    AuthClass.prototype.verifyCurrentUserAttribute = function (attr) {
        var that = this;
        return that
            .currentUserPoolUser()
            .then(function (user) { return that.verifyUserAttribute(user, attr); });
    };
    /**
     * Confirm current user's attribute using a confirmation code
     * @param {Object} attr - The attribute to be verified
     * @param {String} code - The confirmation code
     * @return - A promise resolves to callback data if success
     */
    AuthClass.prototype.verifyCurrentUserAttributeSubmit = function (attr, code) {
        var that = this;
        return that
            .currentUserPoolUser()
            .then(function (user) { return that.verifyUserAttributeSubmit(user, attr, code); });
    };
    AuthClass.prototype.cognitoIdentitySignOut = function (opts, user) {
        return __awaiter(this, void 0, void 0, function () {
            var e_10, isSignedInHostedUI;
            var _this = this;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        _a.trys.push([0, 2, , 3]);
                        return [4 /*yield*/, this._storageSync];
                    case 1:
                        _a.sent();
                        return [3 /*break*/, 3];
                    case 2:
                        e_10 = _a.sent();
                        logger.debug('Failed to sync cache info into memory', e_10);
                        throw e_10;
                    case 3:
                        isSignedInHostedUI = this._oAuthHandler &&
                            this._storage.getItem('amplify-signin-with-hostedUI') === 'true';
                        return [2 /*return*/, new Promise(function (res, rej) {
                                if (opts && opts.global) {
                                    logger.debug('user global sign out', user);
                                    // in order to use global signout
                                    // we must validate the user as an authenticated user by using getSession
                                    var clientMetadata = _this._config.clientMetadata; // TODO: verify behavior if this is override during signIn
                                    user.getSession(function (err, result) { return __awaiter(_this, void 0, void 0, function () {
                                        var cleanUpError_8;
                                        var _this = this;
                                        return __generator(this, function (_a) {
                                            switch (_a.label) {
                                                case 0:
                                                    if (!err) return [3 /*break*/, 5];
                                                    logger.debug('failed to get the user session', err);
                                                    if (!this.isSessionInvalid(err)) return [3 /*break*/, 4];
                                                    _a.label = 1;
                                                case 1:
                                                    _a.trys.push([1, 3, , 4]);
                                                    return [4 /*yield*/, this.cleanUpInvalidSession(user)];
                                                case 2:
                                                    _a.sent();
                                                    return [3 /*break*/, 4];
                                                case 3:
                                                    cleanUpError_8 = _a.sent();
                                                    rej(new Error("Session is invalid due to: " + err.message + " and failed to clean up invalid session: " + cleanUpError_8.message));
                                                    return [2 /*return*/];
                                                case 4: return [2 /*return*/, rej(err)];
                                                case 5:
                                                    user.globalSignOut({
                                                        onSuccess: function (data) {
                                                            logger.debug('global sign out success');
                                                            if (isSignedInHostedUI) {
                                                                _this.oAuthSignOutRedirect(res, rej);
                                                            }
                                                            else {
                                                                return res();
                                                            }
                                                        },
                                                        onFailure: function (err) {
                                                            logger.debug('global sign out failed', err);
                                                            return rej(err);
                                                        },
                                                    });
                                                    return [2 /*return*/];
                                            }
                                        });
                                    }); }, { clientMetadata: clientMetadata });
                                }
                                else {
                                    logger.debug('user sign out', user);
                                    user.signOut(function () {
                                        if (isSignedInHostedUI) {
                                            _this.oAuthSignOutRedirect(res, rej);
                                        }
                                        else {
                                            return res();
                                        }
                                    });
                                }
                            })];
                }
            });
        });
    };
    AuthClass.prototype.oAuthSignOutRedirect = function (resolve, reject) {
        var isBrowser = JS.browserOrNode().isBrowser;
        if (isBrowser) {
            this.oAuthSignOutRedirectOrReject(reject);
        }
        else {
            this.oAuthSignOutAndResolve(resolve);
        }
    };
    AuthClass.prototype.oAuthSignOutAndResolve = function (resolve) {
        this._oAuthHandler.signOut();
        resolve();
    };
    AuthClass.prototype.oAuthSignOutRedirectOrReject = function (reject) {
        this._oAuthHandler.signOut(); // this method redirects url
        // App should be redirected to another url otherwise it will reject
        setTimeout(function () { return reject(Error('Signout timeout fail')); }, 3000);
    };
    /**
     * Sign out method
     * @
     * @return - A promise resolved if success
     */
    AuthClass.prototype.signOut = function (opts) {
        return __awaiter(this, void 0, void 0, function () {
            var user;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        _a.trys.push([0, 2, , 3]);
                        return [4 /*yield*/, this.cleanCachedItems()];
                    case 1:
                        _a.sent();
                        return [3 /*break*/, 3];
                    case 2:
                        _a.sent();
                        logger.debug('failed to clear cached items');
                        return [3 /*break*/, 3];
                    case 3:
                        if (!this.userPool) return [3 /*break*/, 7];
                        user = this.userPool.getCurrentUser();
                        if (!user) return [3 /*break*/, 5];
                        return [4 /*yield*/, this.cognitoIdentitySignOut(opts, user)];
                    case 4:
                        _a.sent();
                        return [3 /*break*/, 6];
                    case 5:
                        logger.debug('no current Cognito user');
                        _a.label = 6;
                    case 6: return [3 /*break*/, 8];
                    case 7:
                        logger.debug('no Cognito User pool');
                        _a.label = 8;
                    case 8:
                        /**
                         * Note for future refactor - no reliable way to get username with
                         * Cognito User Pools vs Identity when federating with Social Providers
                         * This is why we need a well structured session object that can be inspected
                         * and information passed back in the message below for Hub dispatch
                         */
                        dispatchAuthEvent('signOut', this.user, "A user has been signed out");
                        this.user = null;
                        return [2 /*return*/];
                }
            });
        });
    };
    AuthClass.prototype.cleanCachedItems = function () {
        return __awaiter(this, void 0, void 0, function () {
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: 
                    // clear cognito cached item
                    return [4 /*yield*/, this.Credentials.clear()];
                    case 1:
                        // clear cognito cached item
                        _a.sent();
                        return [2 /*return*/];
                }
            });
        });
    };
    /**
     * Change a password for an authenticated user
     * @param {Object} user - The CognitoUser object
     * @param {String} oldPassword - the current password
     * @param {String} newPassword - the requested new password
     * @return - A promise resolves if success
     */
    AuthClass.prototype.changePassword = function (user, oldPassword, newPassword, clientMetadata) {
        var _this = this;
        if (clientMetadata === void 0) { clientMetadata = this._config.clientMetadata; }
        return new Promise(function (resolve, reject) {
            _this.userSession(user).then(function (session) {
                user.changePassword(oldPassword, newPassword, function (err, data) {
                    if (err) {
                        logger.debug('change password failure', err);
                        return reject(err);
                    }
                    else {
                        return resolve(data);
                    }
                }, clientMetadata);
            });
        });
    };
    /**
     * Initiate a forgot password request
     * @param {String} username - the username to change password
     * @return - A promise resolves if success
     */
    AuthClass.prototype.forgotPassword = function (username, clientMetadata) {
        if (clientMetadata === void 0) { clientMetadata = this._config.clientMetadata; }
        if (!this.userPool) {
            return this.rejectNoUserPool();
        }
        if (!username) {
            return this.rejectAuthError(AuthErrorTypes.EmptyUsername);
        }
        var user = this.createCognitoUser(username);
        return new Promise(function (resolve, reject) {
            user.forgotPassword({
                onSuccess: function () {
                    resolve();
                    return;
                },
                onFailure: function (err) {
                    logger.debug('forgot password failure', err);
                    dispatchAuthEvent('forgotPassword_failure', err, username + " forgotPassword failed");
                    reject(err);
                    return;
                },
                inputVerificationCode: function (data) {
                    dispatchAuthEvent('forgotPassword', user, username + " has initiated forgot password flow");
                    resolve(data);
                    return;
                },
            }, clientMetadata);
        });
    };
    /**
     * Confirm a new password using a confirmation Code
     * @param {String} username - The username
     * @param {String} code - The confirmation code
     * @param {String} password - The new password
     * @return - A promise that resolves if success
     */
    AuthClass.prototype.forgotPasswordSubmit = function (username, code, password, clientMetadata) {
        if (clientMetadata === void 0) { clientMetadata = this._config.clientMetadata; }
        if (!this.userPool) {
            return this.rejectNoUserPool();
        }
        if (!username) {
            return this.rejectAuthError(AuthErrorTypes.EmptyUsername);
        }
        if (!code) {
            return this.rejectAuthError(AuthErrorTypes.EmptyCode);
        }
        if (!password) {
            return this.rejectAuthError(AuthErrorTypes.EmptyPassword);
        }
        var user = this.createCognitoUser(username);
        return new Promise(function (resolve, reject) {
            user.confirmPassword(code, password, {
                onSuccess: function (success) {
                    dispatchAuthEvent('forgotPasswordSubmit', user, username + " forgotPasswordSubmit successful");
                    resolve(success);
                    return;
                },
                onFailure: function (err) {
                    dispatchAuthEvent('forgotPasswordSubmit_failure', err, username + " forgotPasswordSubmit failed");
                    reject(err);
                    return;
                },
            }, clientMetadata);
        });
    };
    /**
     * Get user information
     * @async
     * @return {Object }- current User's information
     */
    AuthClass.prototype.currentUserInfo = function () {
        return __awaiter(this, void 0, void 0, function () {
            var source, user, attributes, userAttrs, credentials, e_12, info, err_1, user;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        source = this.Credentials.getCredSource();
                        if (!(!source || source === 'aws' || source === 'userPool')) return [3 /*break*/, 9];
                        return [4 /*yield*/, this.currentUserPoolUser().catch(function (err) {
                                return logger.error(err);
                            })];
                    case 1:
                        user = _a.sent();
                        if (!user) {
                            return [2 /*return*/, null];
                        }
                        _a.label = 2;
                    case 2:
                        _a.trys.push([2, 8, , 9]);
                        return [4 /*yield*/, this.userAttributes(user)];
                    case 3:
                        attributes = _a.sent();
                        userAttrs = this.attributesToObject(attributes);
                        credentials = null;
                        _a.label = 4;
                    case 4:
                        _a.trys.push([4, 6, , 7]);
                        return [4 /*yield*/, this.currentCredentials()];
                    case 5:
                        credentials = _a.sent();
                        return [3 /*break*/, 7];
                    case 6:
                        e_12 = _a.sent();
                        logger.debug('Failed to retrieve credentials while getting current user info', e_12);
                        return [3 /*break*/, 7];
                    case 7:
                        info = {
                            id: credentials ? credentials.identityId : undefined,
                            username: user.getUsername(),
                            attributes: userAttrs,
                        };
                        return [2 /*return*/, info];
                    case 8:
                        err_1 = _a.sent();
                        logger.error('currentUserInfo error', err_1);
                        return [2 /*return*/, {}];
                    case 9:
                        if (source === 'federated') {
                            user = this.user;
                            return [2 /*return*/, user ? user : {}];
                        }
                        return [2 /*return*/];
                }
            });
        });
    };
    AuthClass.prototype.federatedSignIn = function (providerOrOptions, response, user) {
        return __awaiter(this, void 0, void 0, function () {
            var options, provider, customState, client_id, redirect_uri, provider, loggedInUser, token, identity_id, expires_at, credentials, currentUser;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        if (!this._config.identityPoolId && !this._config.userPoolId) {
                            throw new Error("Federation requires either a User Pool or Identity Pool in config");
                        }
                        // Ensure backwards compatability
                        if (typeof providerOrOptions === 'undefined') {
                            if (this._config.identityPoolId && !this._config.userPoolId) {
                                throw new Error("Federation with Identity Pools requires tokens passed as arguments");
                            }
                        }
                        if (!(isFederatedSignInOptions(providerOrOptions) ||
                            isFederatedSignInOptionsCustom(providerOrOptions) ||
                            hasCustomState(providerOrOptions) ||
                            typeof providerOrOptions === 'undefined')) return [3 /*break*/, 1];
                        options = providerOrOptions || {
                            provider: CognitoHostedUIIdentityProvider.Cognito,
                        };
                        provider = isFederatedSignInOptions(options)
                            ? options.provider
                            : options.customProvider;
                        customState = isFederatedSignInOptions(options)
                            ? options.customState
                            : options.customState;
                        if (this._config.userPoolId) {
                            client_id = isCognitoHostedOpts(this._config.oauth)
                                ? this._config.userPoolWebClientId
                                : this._config.oauth.clientID;
                            redirect_uri = isCognitoHostedOpts(this._config.oauth)
                                ? this._config.oauth.redirectSignIn
                                : this._config.oauth.redirectUri;
                            this._oAuthHandler.oauthSignIn(this._config.oauth.responseType, this._config.oauth.domain, redirect_uri, client_id, provider, customState);
                        }
                        return [3 /*break*/, 4];
                    case 1:
                        provider = providerOrOptions;
                        // To check if the user is already logged in
                        try {
                            loggedInUser = JSON.stringify(JSON.parse(this._storage.getItem('aws-amplify-federatedInfo')).user);
                            if (loggedInUser) {
                                logger.warn("There is already a signed in user: " + loggedInUser + " in your app.\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tYou should not call Auth.federatedSignIn method again as it may cause unexpected behavior.");
                            }
                        }
                        catch (e) { }
                        token = response.token, identity_id = response.identity_id, expires_at = response.expires_at;
                        return [4 /*yield*/, this.Credentials.set({ provider: provider, token: token, identity_id: identity_id, user: user, expires_at: expires_at }, 'federation')];
                    case 2:
                        credentials = _a.sent();
                        return [4 /*yield*/, this.currentAuthenticatedUser()];
                    case 3:
                        currentUser = _a.sent();
                        dispatchAuthEvent('signIn', currentUser, "A user " + currentUser.username + " has been signed in");
                        logger.debug('federated sign in credentials', credentials);
                        return [2 /*return*/, credentials];
                    case 4: return [2 /*return*/];
                }
            });
        });
    };
    /**
     * Used to complete the OAuth flow with or without the Cognito Hosted UI
     * @param {String} URL - optional parameter for customers to pass in the response URL
     */
    AuthClass.prototype._handleAuthResponse = function (URL) {
        return __awaiter(this, void 0, void 0, function () {
            var currentUrl, hasCodeOrError, hasTokenOrError, _a, accessToken, idToken, refreshToken, state, session, credentials, isCustomStateIncluded, currentUser, customState, err_2;
            return __generator(this, function (_b) {
                switch (_b.label) {
                    case 0:
                        if (this.oAuthFlowInProgress) {
                            logger.debug("Skipping URL " + URL + " current flow in progress");
                            return [2 /*return*/];
                        }
                        _b.label = 1;
                    case 1:
                        _b.trys.push([1, , 8, 9]);
                        this.oAuthFlowInProgress = true;
                        if (!this._config.userPoolId) {
                            throw new Error("OAuth responses require a User Pool defined in config");
                        }
                        dispatchAuthEvent('parsingCallbackUrl', { url: URL }, "The callback url is being parsed");
                        currentUrl = URL || (JS.browserOrNode().isBrowser ? window.location.href : '');
                        hasCodeOrError = !!(urlParse(currentUrl).query || '')
                            .split('&')
                            .map(function (entry) { return entry.split('='); })
                            .find(function (_a) {
                            var _b = __read(_a, 1), k = _b[0];
                            return k === 'code' || k === 'error';
                        });
                        hasTokenOrError = !!(urlParse(currentUrl).hash || '#')
                            .substr(1)
                            .split('&')
                            .map(function (entry) { return entry.split('='); })
                            .find(function (_a) {
                            var _b = __read(_a, 1), k = _b[0];
                            return k === 'access_token' || k === 'error';
                        });
                        if (!(hasCodeOrError || hasTokenOrError)) return [3 /*break*/, 7];
                        this._storage.setItem('amplify-redirected-from-hosted-ui', 'true');
                        _b.label = 2;
                    case 2:
                        _b.trys.push([2, 6, , 7]);
                        return [4 /*yield*/, this._oAuthHandler.handleAuthResponse(currentUrl)];
                    case 3:
                        _a = _b.sent(), accessToken = _a.accessToken, idToken = _a.idToken, refreshToken = _a.refreshToken, state = _a.state;
                        session = new CognitoUserSession({
                            IdToken: new CognitoIdToken({ IdToken: idToken }),
                            RefreshToken: new CognitoRefreshToken({
                                RefreshToken: refreshToken,
                            }),
                            AccessToken: new CognitoAccessToken({
                                AccessToken: accessToken,
                            }),
                        });
                        credentials = void 0;
                        if (!this._config.identityPoolId) return [3 /*break*/, 5];
                        return [4 /*yield*/, this.Credentials.set(session, 'session')];
                    case 4:
                        credentials = _b.sent();
                        logger.debug('AWS credentials', credentials);
                        _b.label = 5;
                    case 5:
                        isCustomStateIncluded = /-/.test(state);
                        currentUser = this.createCognitoUser(session.getIdToken().decodePayload()['cognito:username']);
                        // This calls cacheTokens() in Cognito SDK
                        currentUser.setSignInUserSession(session);
                        if (window && typeof window.history !== 'undefined') {
                            window.history.replaceState({}, null, this._config.oauth.redirectSignIn);
                        }
                        dispatchAuthEvent('signIn', currentUser, "A user " + currentUser.getUsername() + " has been signed in");
                        dispatchAuthEvent('cognitoHostedUI', currentUser, "A user " + currentUser.getUsername() + " has been signed in via Cognito Hosted UI");
                        if (isCustomStateIncluded) {
                            customState = state.split('-').splice(1).join('-');
                            dispatchAuthEvent('customOAuthState', urlSafeDecode(customState), "State for user " + currentUser.getUsername());
                        }
                        //#endregion
                        return [2 /*return*/, credentials];
                    case 6:
                        err_2 = _b.sent();
                        logger.debug('Error in cognito hosted auth response', err_2);
                        // Just like a successful handling of `?code`, replace the window history to "dispose" of the `code`.
                        // Otherwise, reloading the page will throw errors as the `code` has already been spent.
                        if (window && typeof window.history !== 'undefined') {
                            window.history.replaceState({}, null, this._config.oauth.redirectSignIn);
                        }
                        dispatchAuthEvent('signIn_failure', err_2, "The OAuth response flow failed");
                        dispatchAuthEvent('cognitoHostedUI_failure', err_2, "A failure occurred when returning to the Cognito Hosted UI");
                        dispatchAuthEvent('customState_failure', err_2, "A failure occurred when returning state");
                        return [3 /*break*/, 7];
                    case 7: return [3 /*break*/, 9];
                    case 8:
                        this.oAuthFlowInProgress = false;
                        return [7 /*endfinally*/];
                    case 9: return [2 /*return*/];
                }
            });
        });
    };
    /**
     * Compact version of credentials
     * @param {Object} credentials
     * @return {Object} - Credentials
     */
    AuthClass.prototype.essentialCredentials = function (credentials) {
        return {
            accessKeyId: credentials.accessKeyId,
            sessionToken: credentials.sessionToken,
            secretAccessKey: credentials.secretAccessKey,
            identityId: credentials.identityId,
            authenticated: credentials.authenticated,
        };
    };
    AuthClass.prototype.attributesToObject = function (attributes) {
        var _this = this;
        var obj = {};
        if (attributes) {
            attributes.map(function (attribute) {
                if (attribute.Name === 'email_verified' ||
                    attribute.Name === 'phone_number_verified') {
                    obj[attribute.Name] =
                        _this.isTruthyString(attribute.Value) || attribute.Value === true;
                }
                else {
                    obj[attribute.Name] = attribute.Value;
                }
            });
        }
        return obj;
    };
    AuthClass.prototype.isTruthyString = function (value) {
        return (typeof value.toLowerCase === 'function' && value.toLowerCase() === 'true');
    };
    AuthClass.prototype.createCognitoUser = function (username) {
        var userData = {
            Username: username,
            Pool: this.userPool,
        };
        userData.Storage = this._storage;
        var authenticationFlowType = this._config.authenticationFlowType;
        var user = new CognitoUser(userData);
        if (authenticationFlowType) {
            user.setAuthenticationFlowType(authenticationFlowType);
        }
        return user;
    };
    AuthClass.prototype._isValidAuthStorage = function (obj) {
        // We need to check if the obj has the functions of Storage
        return (!!obj &&
            typeof obj.getItem === 'function' &&
            typeof obj.setItem === 'function' &&
            typeof obj.removeItem === 'function' &&
            typeof obj.clear === 'function');
    };
    AuthClass.prototype.noUserPoolErrorHandler = function (config) {
        if (config) {
            if (!config.userPoolId || !config.identityPoolId) {
                return AuthErrorTypes.MissingAuthConfig;
            }
        }
        return AuthErrorTypes.NoConfig;
    };
    AuthClass.prototype.rejectAuthError = function (type) {
        return Promise.reject(new AuthError(type));
    };
    AuthClass.prototype.rejectNoUserPool = function () {
        var type = this.noUserPoolErrorHandler(this._config);
        return Promise.reject(new NoUserPoolError(type));
    };
    AuthClass.prototype.rememberDevice = function () {
        return __awaiter(this, void 0, void 0, function () {
            var currUser, error_1;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        _a.trys.push([0, 2, , 3]);
                        return [4 /*yield*/, this.currentUserPoolUser()];
                    case 1:
                        currUser = _a.sent();
                        return [3 /*break*/, 3];
                    case 2:
                        error_1 = _a.sent();
                        logger.debug('The user is not authenticated by the error', error_1);
                        return [2 /*return*/, Promise.reject('The user is not authenticated')];
                    case 3:
                        currUser.getCachedDeviceKeyAndPassword();
                        return [2 /*return*/, new Promise(function (res, rej) {
                                currUser.setDeviceStatusRemembered({
                                    onSuccess: function (data) {
                                        res(data);
                                    },
                                    onFailure: function (err) {
                                        if (err.code === 'InvalidParameterException') {
                                            rej(new AuthError(AuthErrorTypes.DeviceConfig));
                                        }
                                        else if (err.code === 'NetworkError') {
                                            rej(new AuthError(AuthErrorTypes.NetworkError));
                                        }
                                        else {
                                            rej(err);
                                        }
                                    },
                                });
                            })];
                }
            });
        });
    };
    AuthClass.prototype.forgetDevice = function () {
        return __awaiter(this, void 0, void 0, function () {
            var currUser, error_2;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        _a.trys.push([0, 2, , 3]);
                        return [4 /*yield*/, this.currentUserPoolUser()];
                    case 1:
                        currUser = _a.sent();
                        return [3 /*break*/, 3];
                    case 2:
                        error_2 = _a.sent();
                        logger.debug('The user is not authenticated by the error', error_2);
                        return [2 /*return*/, Promise.reject('The user is not authenticated')];
                    case 3:
                        currUser.getCachedDeviceKeyAndPassword();
                        return [2 /*return*/, new Promise(function (res, rej) {
                                currUser.forgetDevice({
                                    onSuccess: function (data) {
                                        res(data);
                                    },
                                    onFailure: function (err) {
                                        if (err.code === 'InvalidParameterException') {
                                            rej(new AuthError(AuthErrorTypes.DeviceConfig));
                                        }
                                        else if (err.code === 'NetworkError') {
                                            rej(new AuthError(AuthErrorTypes.NetworkError));
                                        }
                                        else {
                                            rej(err);
                                        }
                                    },
                                });
                            })];
                }
            });
        });
    };
    AuthClass.prototype.fetchDevices = function () {
        return __awaiter(this, void 0, void 0, function () {
            var currUser, error_3;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        _a.trys.push([0, 2, , 3]);
                        return [4 /*yield*/, this.currentUserPoolUser()];
                    case 1:
                        currUser = _a.sent();
                        return [3 /*break*/, 3];
                    case 2:
                        error_3 = _a.sent();
                        logger.debug('The user is not authenticated by the error', error_3);
                        throw new Error('The user is not authenticated');
                    case 3:
                        currUser.getCachedDeviceKeyAndPassword();
                        return [2 /*return*/, new Promise(function (res, rej) {
                                var cb = {
                                    onSuccess: function (data) {
                                        var deviceList = data.Devices.map(function (device) {
                                            var deviceName = device.DeviceAttributes.find(function (_a) {
                                                var Name = _a.Name;
                                                return Name === 'device_name';
                                            }) || {};
                                            var deviceInfo = {
                                                id: device.DeviceKey,
                                                name: deviceName.Value,
                                            };
                                            return deviceInfo;
                                        });
                                        res(deviceList);
                                    },
                                    onFailure: function (err) {
                                        if (err.code === 'InvalidParameterException') {
                                            rej(new AuthError(AuthErrorTypes.DeviceConfig));
                                        }
                                        else if (err.code === 'NetworkError') {
                                            rej(new AuthError(AuthErrorTypes.NetworkError));
                                        }
                                        else {
                                            rej(err);
                                        }
                                    },
                                };
                                currUser.listDevices(MAX_DEVICES, null, cb);
                            })];
                }
            });
        });
    };
    return AuthClass;
}());
var Auth = new AuthClass(null);
Amplify.register(Auth);

/*
 * Copyright 2017-2017 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"). You may not use this file except in compliance with
 * the License. A copy of the License is located at
 *
 *     http://aws.amazon.com/apache2.0/
 *
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
 * and limitations under the License.
 */
/**
 * @deprecated use named import
 */
var __pika_web_default_export_for_treeshaking__ = Auth;

export { __pika_web_default_export_for_treeshaking__ as default };
