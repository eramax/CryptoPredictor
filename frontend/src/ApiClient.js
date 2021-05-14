import * as signalR from "@microsoft/signalr";
export default class ApiClient {
    constructor(host, username, password) {
        console.log("ApiClient");
        this.host = host;
        this.username = username;
        this.password = password;
        this.connection = null;
        this.expires = null;
    }
    async GetAccessToken() {
        if (this.expires != null && Date.now() < this.expires) return;
        console.log("GetAccessToken")
        const response = await fetch(
            `https://${this.host}:5000/api/auth/access_token`, {
                retries: 3,
                retryDelay: 1000,
                method: "POST",
                headers: {
                    Accept: "application/json",
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    Username: this.username,
                    Password: this.password
                }),
            }
        );
        const data = await response.json();
        this.token = data.access_token;
        this.expires = data.expires;
    }

    async LoadData(currency, tframe, end, limit) {
        await this.GetAccessToken();
        console.log("LoadData")
        const response = await fetch(`https://${this.host}:5000/api/data/${currency}?tframe=${tframe}&limit=${limit}&endDate=${end}`, {
            retries: 3,
            retryDelay: 1000,
            method: "GET",
            headers: new Headers({
                Authorization: "Bearer " + this.token,
            }),
        })
        const json = await response.json();
        return json;
    }

    async LoadForcast(currency, tframe, end, limit) {
        await this.GetAccessToken();
        console.log("LoadForcast")
        const response = await fetch(`https://${this.host}:5000/api/forcast/${currency}?tframe=${tframe}&limit=${limit}&endDate=${end}`, {
            retries: 3,
            retryDelay: 1000,
            method: "GET",
            headers: new Headers({
                Authorization: "Bearer " + this.token,
            }),
        })
        const json = await response.json();
        return json;
    }

    async Connect() {
        if (this.connection != null && this.connection.state == "Connected") return;
        console.log("New connection for SignalR")
        this.connection = new signalR.HubConnectionBuilder()
            .configureLogging(signalR.LogLevel.Debug)
            .withUrl(`https://${this.host}:5000/ws`, {
                accessTokenFactory: () => this.token,
            })
            .withAutomaticReconnect([0, 0, 10000])
            .build();
        this.connection.onreconnecting((error) =>
            console.log(`Connection lost due to error "${error}". Reconnecting.`)
        );
        await this.connection.start();
        window.connection = this.connection;
    }
    async Subscribe(currency, tframe, DataChange) {
        await this.Connect();
        let topic = `${currency}/${tframe}`
        this.connection.on(topic, DataChange);
        this.connection.invoke("Subscribe", topic).catch(console.log);
        console.log(`Subscribed to ${topic}`)
    }
    async Unubscribe(currency, tframe) {
        if (this.connection == null || this.connection.state != "Connected") return;
        let topic = `${currency}/${tframe}`
        this.connection.invoke("Unsubscribe", topic).catch(console.log);
        console.log(`Unsubscribed to ${topic}`)
    }
}