<?php
$GameVersion = "1.7";
$hostName    = 'fdb30.awardspace.net';
$dbName      = '3675070_fps';
$dbUser      = '3675070_fps';
$dbPassworld = '#Fergie440';
$secretKey   = "123456";
$base_url    = 'http://www.blacklegendsfps.atwebpages.com/php/';
$emailFrom   = 'example@gmail.com';
$GameName    = "Game Name Here";

function dbConnect()
{
    global $dbName;
    global $secretKey;
    global $hostName;
    global $dbUser;
    global $dbPassworld;
    
    $link = mysqli_connect($hostName, $dbUser, $dbPassworld, $dbName);
    
    if (!$link) {
        fail("Couldn´t connect to database server: " . mysqli_connect_error());
    }
    
    return $link;
}

function TrydbConnect()
{
    global $dbName;
    global $secretKey;
    global $hostName;
    global $dbUser;
    global $dbPassworld;
    
    $link = @mysqli_connect($hostName, $dbUser, $dbPassworld, $dbName) or die("2");
    return $link;
}

function safe($variable)
{
    $variable = addslashes(trim($variable));
    return $variable;
}

function fail($errorMsg)
{
    print $errorMsg;
    exit;
}

function EchoWithPrefix($content){
    echo "success".$content;
}
?>