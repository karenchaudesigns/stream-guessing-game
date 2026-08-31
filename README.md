# **Twitch Chat Guessing Game Overlay**

An interactive, automated guessing game built for Twitch streams using [Streamer.bot](https://streamer.bot/) and an OBS HTML overlay.  
This project allows a broadcaster to secretly whisper a choice of three random words to a guest (or themselves). Once the guest locks in their choice, chat can guess the word in real-time. The first chatter to guess correctly triggers an on-screen confetti celebration and earns points on a dynamic, live-updating OBS scoreboard.

## **Features**

* **Automated Word Generation:** Fetches 3 random words from a local `words.txt` file and whispers them to the player.
* **Seamless Twitch Integration:** Fully contained within Streamer.bot—no manual Discord DMs or third-party scorekeeping required.
* **Real-time Leaderboard:** Tracks points during the stream and updates a sliding leaderboard instantly.
* **Animated OBS Overlay:** Built with HTML, Tailwind CSS, and vanilla JavaScript (no external animation libraries needed). Includes a custom particle confetti system.
* **Format Agnostic:** Perfect for drawing games, sculpting, charades, or any creative guessing format.

## **Prerequisites**

1. **Streamer.bot** (v0.2.0 or higher recommended)
2. **OBS Studio** (or any broadcasting software that supports Browser Sources)
3. A Twitch Bot account linked to Streamer.bot (must have a verified phone number to send automated whispers).

## **Setup Instructions**

### **1. The OBS Overlays**

The project includes a unified overlay as well as modular components to fit your stream layout. Since the HTML files connect to Streamer.bot's local WebSocket server (127.0.0.1:8080), you should load them as a "Local File" in OBS. Hosting the overlays on HTTPS (like GitHub Pages) will cause the browser to block the unencrypted connection to the local WebSocket.

Available overlays:
* `index.html`: A unified overlay containing all game elements.
* `goose_clue_overlay.html`: An overlay focusing on the current game state, the active guest providing clues, and the clues themselves.
* `sunny_pond_announcement.html`: An overlay for announcing the game winner and the correct word.
* `sunny_pond_leaderboard.html`: An overlay specifically for displaying the ongoing leaderboard.
* `timer_widget.html`: A visual countdown timer for the game.

To set up an overlay:
1. Clone or download this repository to your computer.
2. In Streamer.bot, go to the **Servers/Clients** tab -> **WebSocket Server** and ensure "Auto Start" is checked and running on Port 8080.
3. In OBS, add a new **Browser Source**.
4. Check the "Local file" box and browse to the desired HTML file (e.g., `index.html`) on your computer.
5. Set Width to 1920 and Height to 1080 (or adjust according to the specific modular widget size).

### **2. Streamer.bot Configuration**

You will need to create three separate Actions in Streamer.bot.

#### **Action 1: Generate Options**

* **Name:** GiveWordOptions
* **Trigger:** Twitch -> Chat -> Command (Create a command like !giveword and restrict it to Broadcaster/Moderator).
* **Sub-Action:** Core -> C# -> Execute C# Code.
* **Code:** Paste the contents of give_word_options.cs. *(Note: You must add System.dll in the references tab of the C# compiler).*

#### **Action 2: Lock In Whisper**

* **Name:** ProcessWhisper
* **Trigger:** Twitch -> Chat -> Bot Whispers.
* **Sub-Action:** Core -> C# -> Execute C# Code.
* **Code:** Paste the contents of process_whisper.cs.

#### **Action 3: Process Chat Guesses**

* **Name:** CheckChatGuess
* **Trigger:** Twitch -> Chat -> Chat Message.
* **Sub-Action:** Core -> C# -> Execute C# Code.
* **Code:** Paste the contents of check_chat_guess.cs.

#### **Action 4: Timer Finished**

* **Name:** Timer Finished
* **Trigger:** None (Triggered by frontend `DoAction` WebSocket request when the timer hits zero).
* **Sub-Action:** Core -> C# -> Execute C# Code.
* **Code:** Paste the contents of timer_finished.cs.

## **How to Play Live**

1. **Start the Game:** Type !giveword \[username] in your Twitch chat.
2. **The Whisper:** Streamer.bot will securely whisper 3 options to that user (e.g., *A - Keyboard B - Telescope C - Apple*).
3. **The Lock-In:** The user replies to the whisper with exactly A, B, or C.
4. **The Game Begins:** The bot confirms the word in the whisper and announces in chat that the game has started.
5. **The Win:** Chatters guess words wildly. The moment someone types the exact word, the OBS overlay triggers, their name and points are added to the leaderboard, and the secret word is cleared to prevent duplicate winners.
6. **The Goose Forfeits:** If the clue giver (the Goose) types the secret word in chat, they forfeit the game, no one wins, and the round ends.

## **Global Variables**

The overlays react to the following global variables broadcasted by Streamer.bot over the WebSocket connection:

* `guessing-game_state`: Controls the UI mode (e.g., `inactive`, `waiting`, `active`, `recap`).
* `guessing-game_currentGuest`: The Twitch username of the person playing / giving clues.
* `guessing-game_gooseClue`: The current clue given by the guest.
* `guessing-game_leaderboard`: A JSON string representing the current player scores.
* `guessing-game_winner`: The Twitch username of the winning guesser.
* `guessing-game_winningWord`: The word that was successfully guessed.
* `guessing-game_secretWord`: The active secret word (used internally and to trigger the timer via a custom event).
* `guessing-game_timerDuration`: The length of the timer in minutes (read from a custom `GameStart` event).
* `guessing-game_timerStatus`: Status passed back to Streamer.bot when the timer finishes (e.g., `timedout`).

## **Customization**

The overlay is styled using Tailwind CSS via CDN. If you want to change the default aesthetic to match your channel branding (for example, swapping the purple pop-up gradient to your official magenta brand color), simply edit the Tailwind classes in the index.html file.  
Look for this line in the HTML:  
<div id="winner-popup" class="bg-gradient-to-r from-purple-600 to-blue-600...">

