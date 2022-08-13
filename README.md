# Multicast-Middleware-Program

I used C# WPF to create the GUI of the middleware program. Each middleware then used TCP to connect to the network. For every middleware I created an if statement to filter the different messages that is coming in. For example, any message with “\<EOM\>” is coming from the network and will be used to put into the Received list box and the holdQueue. Every middleware then sends out their timestamp when they received the broadcast message with “at” in the message. When the middlewares receive all the “at” messages for their message, the largest is assigned to that message and broadcasted to the other middlewares with “largest” in the message to indicate that the timestamp is the largest for that message. Every middleware will update to the largest and update the holdQueue before sending the message to the deliveryQueue for the Ready box.

The holdQueue stores each message in a dictionary with the message ID and a timestamp. The message id is the middleware number plus a space and the message number. Using string manipulation such as Split() to retrieve the two numbers if required. The time stamp is the value, and the message id is the key of the dictionary. Every time a larger timestamp is received, it will update, and the final agreed timestamp will be used. The total order uses a loop to search for the timestamp starting from 0, if there are multiple messages with the same timestamp, then use the priority first. In this case priority is set to the middlewares. Once the total order is achieved, the Ready box will update with the latest order. All middlewares should therefore output the same order.

# Instructions on how to run the program
Each program resides in their respective folders: Middleware 1, Middleware 2 etc.

The batch files allow the programs to execute.

The programs may be executed in any order, but all programs must be running before sending any broadcasts.

There are six batch files. One is for launching all the programs at once, however, this can only be done if the items are in the same directory. The other five batch files launch each program respectively. All batch files must not be moved to different folders.

**For each batch file please set your msbuild location for pasthMSBuild to the msbuild.exe location in the Visual Studio installation folder on your computer. The line looks likes this:**

set ```pathMSBuild="%PROGRAMFILES(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin"```

“runAllBatchFile” should only be used to run all the programs at once when they are in the same directory.

“M1BatchFile” runs the Middleware 1 program.

“M2BatchFile” runs the Middleware 2 program and so on.

You may move the folders to any location, but do not move files in the folders to other folders.
