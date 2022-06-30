// This stub file only exists to circumvent a strange problem with
// JsInterop JSException handling. Errors thrown by the aws-amplify/auth 
// library don't provide information about the JS error in the C# catch
// block. So, we implement a simple error handling strategy where
// exported functions provide a "named" error to the C# caller 
// if the amplify lib call fails.
// 

import Auth from './web_modules/@aws-amplify/auth.js';

export class LzAuth {

    static async configure(authConfig) {
        try { await Auth.configure(authConfig); }
        catch (error) { throw Error(error); }
    }

    // Create a new user in the Amazon Cognito UserPool by passing the new user’s 
    // email address, password, and other attributes to Auth.signUp.
    // To create a custom attribute during your sign-up process, add it to the 
    // attributes field of the signUp method prepended with custom:
    // The Auth.signUp promise returns a data object of type ISignUpResult with a 
    // CognitoUser. CognitoUser contains a userSub which is a unique identifier 
    // of the authenticated user; the userSub is not the same as the username.
    // {
    //  user: CognitoUser;
    //  userConfirmed: boolean;
    //  userSub: string;
    // }
    static async signUp(username, password, attributes) {
        try {
            let cognitoUser = await Auth.signUp({
                username,
                password,
                attributes: attributes,
            });
            //console.log(cognitoUser);
            return cognitoUser;
        } catch (error) { throw Error(error.name); }
    }

    // If you enabled multi-factor auth, confirm the sign-up after retrieving a 
    // confirmation code from the user.
    static async confirmSignUp(username, code) {
        try { await Auth.confirmSignUp(username, code); }
        catch (error) { throw Error(error.name); }
    }

    // You can resend the user a signup confirmation code
    static async resendSignUp(username) {
        try { await Auth.resendSignUp(username); }
        catch (error) { throw Error(error.name); }
    }

    // When signing in with user name and password, you will pass in the username 
    // and the password to the signIn method of the Auth class.
    static async signIn(username, password) {
        try {  !await Auth.signIn(username, password); }
        catch (error) { throw Error(error.name); }
    }

    // Revokes Cognito tokens. 
    static async signOut(globalSignOut = false) {
        try {
            await Auth.signOut({ global: globalSignOut });
        } catch (error) { throw Error(error.name); }
    }

    // Change the currently authenticated user's password
    static async changePassword(oldPassword, newPassword) {
        try {
            let user = await Auth.currentAuthenticatedUser();
            let result = await Auth.changePassword(user, oldPassword, newPassword);
            if (result != "SUCCESS")
                throw Error("Password Change Failed " + result);
        }
        catch (error) { throw Error(error.name); }
    }

    // Send confirmation code to user's email
    static async forgotPassword(username) {
        try { await Auth.forgotPassword(username); }
        catch (error) { throw Error(error.name); }
    }

    // Collect confirmation code and new password, then
    static async forgotPasswordSubmit(username, code, newPassword) {
        try { await Auth.forgotPasswordSubmit(username, code, newPassword); }
        catch (error) { throw Error(error.name); }
    }

    // Respond to new password required 
    // The user is asked to provide the new password and required attributes 
    // during the first sign-in attempt if a valid user directory is created 
    // in Amazon Cognito. During this scenario, the following method can be 
    // called to process the new password entered by the user.
    // Note: In my experience, this method is necessary when the admin 
    // created the user with an initial "temporay" password. On first login
    // the user is required to provide a new password. 
    static async completeNewPassword(username, password) {
        try {
            let user = await Auth.currentAuthenticatedUser();
            await Auth.completeNewPassword(user, password,);
        } catch (error) { throw Error(error.name); }
    }

    // Either the phone number or the email address is required for account recovery.
    // You can use this function to start the verification process. A code is sent
    // to the user.
    static async verifyCurrentUserAttribute(attribute) {
        try { await Auth.verifyCurrentUserAttribute(attribute); }
        catch (error) { throw Error(error.name); }
    }

    // Once the user has supplied a verification code, you can call this function 
    // to validate it matches what Cognito sent them.
    static async verifyCurrentUserAttributeSubmit(attribute, code) {
        try { await Auth.verifyCurrentUserAttributeSubmit(attribute, code); }
        catch (error) { throw Error(error.name); }
    }

    // Get the current authenticated user object
    // bypassCache: boolean
    // {
    //  user: CognitoUser;
    //  userConfirmed: boolean;
    //  userSub: string;
    //  attributes: []
    // }
    // 
    static async currentAuthenticatedUser(bypassCache) {
        try { return await currentAuthenticatedUser({ bypassCache: bypassCache }) }
        catch (error) { throw Error(error.name); }
    }

    // Auth.currentSession() returns a CognitoUserSession object which 
    // contains JWT accessToken, idToken, and refreshToken.
    // {
    //  accessToken: string;
    //  idToken: string;
    //  refreshToken: string;
    // }
    static async currentSession() {
        try { return await Auth.currentSession(); }
        catch (error) { throw Error(error.name); }
    }

    // Update user attributes.
    // If you change the email address, the user will receive a confirmation code and 
    // you will need to call verifyCurrentUserAttributeSubmit()
    static async updateUserAttributes(attributes) {
        try {
            let user = await Auth.currentAuthenticatedUser();
            Auth.updateUserAttributes(user, attributes);
        } catch (error) { throw Error(error.name); }
    }

    //The ID Token contains claims about the identity of the authenticated user 
    // such as name, email, and phone_number.It could have custom claims as well, 
    // for example using Amplify CLI. On the Amplify Authentication category you can 
    // retrieve the Id Token using:
    static async getIdToken() {
        try {
            return (await Auth.currentSession()).getIdToken().getJwtToken();
        } catch (error) { throw Error(error.name); }
    }

    // The Access Token contains scopes and groups and is used to grant access 
    // to authorized resources.
    // You can retrieve the Access Token using:
    static async getAccessToken() {
        try {
            return (await Auth.currentSession()).getAccessToken().getJwtToken();
        } catch (error) { throw Error(error.name); }
    }

    // Some apps need to use AWS services which require signing requests. 
    // Amplify automatically signs requests with short term credentials 
    // from a Cognito Identity Pool which automatically expire, rotate, 
    // and refresh by the Amplify client libraries.
    static async currentCredentials() {
        try {
            return await Auth.currentCredentials();
        } catch (error) { throw Error(error.name); }
    }
}
