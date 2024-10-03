import { Log, UserManager} from "oidc-client-ts";

Log.setLogger(console);
Log.setLevel(Log.INFO);

const url = window.location.origin + "";

export const settings = {
    authority: "https://dev-2boi5esrnn54g72n.us.auth0.com",
    client_id: "D2MwAZQYBpVoWHJyE5wNJrh7XYZJA1TO",
    redirect_uri: url + "/callback.html",
    post_logout_redirect_uri: url + "/index.html",
    response_type: "code",
    scope: "openid email roles",

    response_mode: "query",

    filterProtocolClaims: true,
    extraQueryParams: {
        audience: "bar-auth0-api",
    },
    api_bar_uri: "http://localhost:5172/api/bar",
    api_manageBar_uri: "http://localhost:5172/api/managebar"
};

export {
    Log,
    UserManager
};
