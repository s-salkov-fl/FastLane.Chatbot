'use strict';

const { Client, LocalAuth } = require('whatsapp-web.js');
const qrcode = require('qrcode-terminal');
const client = new Client({
	authStrategy: new LocalAuth({
		dataPath: 'FastLane.Chatbot.WhatsAppNodeJs'
	})
});

const whatsAppNodeJS = {
	isReady: false,
	lastQr: '',

	authenticateStatus: (callback) => {
		try {
			let result = { lastqr: whatsAppNodeJS.lastQr, isready: whatsAppNodeJS.isReady };

			if (callback) callback(null, result);
			else return result;
		}
		catch (err) {
			if (callback) callback(err.message, null);
			else throw err;
		}
	},
	isReadyClient: (callback) => {
		callback(null, whatsAppNodeJS.isReady);
	},
	getChatUnreads: async () => {

		if (whatsAppNodeJS.isReady) {
			let allChats = await client.getChats();
			let result = [];

			for (let chat of allChats)
			{
				if (chat.unreadCount != 0) {
					result.push({ key: chat.name, value: chat.unreadCount });
				}
			}

			return result;
		}

		throw new Error("Client is not ready. Should check isReadyClient");
	},
	getMessages: async (userId) => {
		if (whatsAppNodeJS.isReady) {
			let allChats = await client.getChats();
			let result = [];
			let targetChat = null;

			for (let chat of allChats) {
				if (chat.name == userId) {
					targetChat = chat;
					break;
				}
			}

			if (targetChat != null) {
				let allContacts = await client.getContacts();

				for (let contact of allContacts) {
					if (contact.name == userId) {
						let chat = await contact.getChat();
						if (chat) {
							targetChat = chat;
							break;
						}
					}
				}
			}

			if (targetChat != null) {
				let messages = await targetChat.fetchMessages();

				for (let message of messages) {
					result.push({ content: message.body, member: (message.fromMe) ? 1 : 2 });
				}

				targetChat.sendSeen();
			}

			return result.reverse();
		}

		throw new Error("Client is not ready. Should check isReadyClient");
	},
	postMessage: async (userId, content) => {
		if (whatsAppNodeJS.isReady) {
			let allChats = await client.getChats();
			let chatFound = false;

			for (let chat of allChats) {
				if (chat.name == userId) {
					chat.sendMessage(content);
					chatFound = true;
					break;
				}
			}

			if (!chatFound) {
				let allContacts = await client.getContacts();

				for (let contact of allContacts) {
					if (contact.name == userId) {
						let chat = await contact.getChat();
						if (chat) {
							chat.sendMessage(content);
							chatFound = true;
							break;
						}
					}
				}
			}

			return chatFound;
		}

		throw new Error("Client is not ready. Should check isReadyClient");
	}
};

module.exports = whatsAppNodeJS;

client.on('qr', (qr) => {
	qrcode.generate(qr, { small: true }, qr => whatsAppNodeJS.lastQr = qr);
});

client.on('ready', () => {
	whatsAppNodeJS.isReady = true;
});

client.initialize();

