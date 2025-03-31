
const socket = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:3000/chathub") // Backend URL
    .build();
socket.start().catch(err => console.error(err.toString()));

const chat = document.getElementById("chat");

socket.on("message", function (user, data) {
    chat.value += data + "\n";
});

socket.on("writing", function (user, data) {
    createUserWritingAlert(data);
});

function createUserWritingAlert(data) {
    const writingAlert = document.createElement("div");
    writingAlert.innerText = data;
    writingAlert.style.color = "blue";
    writingAlert.style.fontStyle = "italic";
    writingAlert.style.fontSize = "12px";
    writingAlert.style.marginTop = "5px";
    writingAlert.style.marginBottom = "5px";
    writingAlert.id = "writingAlert";
    document.querySelector("body").appendChild(writingAlert);
    setTimeout(() => {
        document.querySelector("body").removeChild(writingAlert);
    }, 3000);
}


function sendMessage() {
    const msg = document.getElementById("message").value;
    socket.invoke("message", '', msg).catch(function (err) {
        return console.error(err.toString());
    });

    document.getElementById("message").value = '';
}

function sendWriting() {
    const msg = document.getElementById("message").value;
    socket.invoke("writing", '', 'msg').catch(function (err) {
        return console.error(err.toString());
    });
}

