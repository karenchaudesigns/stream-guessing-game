# **Twitch Chat Guessing Game Overlay**

An interactive, automated guessing game built for Twitch streams using [Streamer.bot](https://streamer.bot/) and an OBS HTML overlay.  
This project allows a broadcaster to secretly whisper a choice of three random words to a guest (or themselves). Once the guest locks in their choice, chat can guess the word in real-time. The first chatter to guess correctly triggers an on-screen confetti celebration and earns points on a dynamic, live-updating OBS scoreboard.

## **Features**

* **Automated Word Generation:** Fetches 3 random words via a free API and whispers them to the player.
* **Seamless Twitch Integration:** Fully contained within Streamer.bot—no manual Discord DMs or third-party scorekeeping required.
* **Real-time Leaderboard:** Tracks points during the stream and updates a sliding leaderboard instantly.
* **Animated OBS Overlay:** Built with HTML, Tailwind CSS, and vanilla JavaScript (no external animation libraries needed). Includes a custom particle confetti system.
* **Format Agnostic:** Perfect for drawing games, sculpting, charades, or any creative guessing format.

## **Prerequisites**

1. **Streamer.bot** (v0.2.0 or higher recommended)
2. **OBS Studio** (or any broadcasting software that supports Browser Sources)
3. A Twitch Bot account linked to Streamer.bot (must have a verified phone number to send automated whispers).

## **Setup Instructions**

### **1. The OBS Overlay**

Since the HTML file connects to Streamer.bot's local WebSocket server (127.0.0.1:8080), you should load the HTML as a "Local File" in OBS. Hosting the overlay on HTTPS (like GitHub Pages) will cause the browser to block the unencrypted connection to the local WebSocket.

1. Clone or download this repository to your computer.
2. In Streamer.bot, go to the **Servers/Clients** tab -> **WebSocket Server** and ensure "Auto Start" is checked and running on Port 8080.
3. In OBS, add a new **Browser Source**.
4. Check the "Local file" box and browse to the `guessing_game_obs_overlay.html` file on your computer.
5. Set Width to 1920 and Height to 1080 (or match your canvas resolution).

### **2. Streamer.bot Configuration**

You will need to create three separate Actions in Streamer.bot.

#### **Action 1: Generate Options**

* **Name:** GiveWordOptions
* **Trigger:** Twitch -> Chat -> Command (Create a command like !giveword and restrict it to Broadcaster/Moderator).
* **Sub-Action:** Core -> C# -> Execute C# Code.
* **Code:** Paste the contents of generate_options_action.cs. *(Note: You must add System.dll and System.Net.Http.dll in the references tab of the C# compiler).*

#### **Action 2: Lock In Whisper**

* **Name:** ProcessWhisper
* **Trigger:** Twitch -> Chat -> Bot Whispers.
* **Sub-Action:** Core -> C# -> Execute C# Code.
* **Code:** Paste the contents of lock_in_whisper_action.cs.

#### **Action 3: Process Chat Guesses**

* **Name:** CheckChatGuess
* **Trigger:** Twitch -> Chat -> Chat Message.
* **Sub-Action:** Core -> C# -> Execute C# Code.
* **Code:** Paste the contents of process_chat_guesses_action.cs.

## **How to Play Live**

1. **Start the Game:** Type !giveword \[username] in your Twitch chat.
2. **The Whisper:** Streamer.bot will securely whisper 3 options to that user (e.g., *A) Keyboard B) Telescope C) Apple*).
3. **The Lock-In:** The user replies to the whisper with exactly A, B, or C.
4. **The Game Begins:** The bot confirms the word in the whisper and announces in chat that the game has started.
5. **The Win:** Chatters guess words wildly. The moment someone types the exact word, the OBS overlay triggers, their name and points are added to the leaderboard, and the secret word is cleared to prevent duplicate winners.

## **Customization**

The overlay is styled using Tailwind CSS via CDN. If you want to change the default aesthetic to match your channel branding (for example, swapping the purple pop-up gradient to your official magenta brand color), simply edit the Tailwind classes in the index.html file.  
Look for this line in the HTML:  
<div id="winner-popup" class="bg-gradient-to-r from-purple-600 to-blue-600...">

