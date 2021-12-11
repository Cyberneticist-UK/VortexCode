# VortexCode
The public repository for the Vortex language

Vortex was not just developed on a whim. It grew out of a set of projects that I was working on that has culminated into a fully inclusive system. It all started with what I hoped would be a PhD for me to take - "Can you build a computer that would last 1000 years?" There are a lot of complexities to this question, but ultimately it was about building a virtual computer that could sit on top of any computer and could be developed easily by anyone with a bit of basic programming knowledge. That would be a book in itself! 

As a teacher, I had tried to develop a scripting language to make games with easily. This language, TornadoScript, worked for games But was never designed to be a general programming language.
What I wanted was a language that was more multi-purpose, more flexible, and could be more useful to people. One thing that has always interested me about programming is that the languages we use are basically all built in English, no matter if that is our native language or not. Surely this makes it harder for someone to learn if English isn't their first language? I know that "Convert" is going to change something into something else, if it was in French and said "Convertir" I could probably guess at what it would do, but if it said "Tiontaigh" I wouldn't have a clue (That's "Convert" in Irish). What if it said "ਤਬਦੀਲ ਕਰੋ" (Punjabi) instead? I would have to learn the shapes and patterns of the words and what they represented before I even start with the complexity of coding.

I came up with a new data storage system that is known as a "hierarchical self describing semantic network" which I term "Netelligence". The concept of this system is that any type of data can be linked to any other, and could automatically display the data in a browser. To get this working in more than just a basic way, I soon realised that there would need to be some form of scripting language that could take the data and manipulate it ready for display.

It is these projects that led to the language that has grown to become Vortex, and it is intrinsically linked to the Netelligence data storage system. There are some really unique features to Vortex that make it a hugely flexible language that can be extended by anyone that wants to, and can be rebuilt for multiple platforms.

Every command is built into a library that is loaded up into the system at runtime, not at compile time. This means that if I want to add new commands, I can do so without having to update the entire system each time. I also allow anyone to make commands as they wish for the language too, so if Vortex doesn't do something that you need it to, you can create your own command (currently in c#, but eventually in whichever base system the engine has been written in).

Each command has a unique identifier associated, so if you wanted to you could take the current commands and rewrite them in a different language / alphabet however you wished, and as long as you kept the same unique identifiers as the English counterpart, someone that had written their script in English could convert it over to the other language. This means people learning the language who are not English speakers are no longer at the same disadvantage as they are in most current languages.

Each command completely describes how it can be used and gives a description of what it does, and these can be accessed by the programmer. You also have a command to list all of the commands. This means that someone could take the current commands and engine and go through all the commands one by one, and rewrite the engine in another language so that as long as the commands all do the same thing, you can make your own engine for a new platform - even one that currently doesn't exist.

Each command library can also have an associated file with it for "legacy" commands. I may, for example, have started off creating the command "Dec" to decrement a counter, but later I may change my mind and want it to be "Var.Dec" to show it works with variables, making a change to the language would normally mean having to go and change all the scripts that use that command. However, I can add into the legacy list that "Dec" maps to "Var.Dec", then if the script says Dec it knows which command to use, and adds an alert to the error log that says that the command needs updating. It also means that tools can be made to automatically update scripts to the latest versions of commands.


If everything's sounding a little complicated, it actually isn't when writing vortex scripts. In fact, there are just ten rules to follow in this language's grammar, and if you can master these, you can build programs in Vortex.

Vortex code starts with <? and ends with ?>
A VTX Script file can contain web code (HTML, CSS, JS, Etc) and Vortex code.
You can have multiple sections of Vortex code in a single file.
Comments in Vortex code are surrounded by the tags <!-- -->
Every Name being used, eg a Variable name, is in speech marks 
If the value of a variable etc is to be used, the name is without speech marks
Every command in Vortex has the same format - the command comes first, then the parameters, finally a semi colon. Vortex is case sensitive. 
Any conditional statements are in smooth brackets
Any sub items / code lines are in curly brackets
Compound commands (where a command is used within a command) are in square brackets

You may ask, what does this all mean? It means that every line of code follows the same pattern, making it much easier for the programmer to read and understand. It means that you can have a single file containing webpage information as well as the server side script. It also means that this is the first language that you can write code in multiple human languages and it can still run! The first language that holds enough information about itself for any future programmer to recreate it and not lose our information.
