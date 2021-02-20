<?php
include("bl_Common.php");
Utils::check_session($_POST['sid']);

$link = Connection::dbConnect();

$sid               = Utils::sanitaze_var($_POST['sid'], $link);
$name              = Utils::sanitaze_var($_POST['name'], $link, $sid);
$nick              = Utils::sanitaze_var($_POST['nick'], $link, $sid);
$password          = Utils::sanitaze_var($_POST['password'], $link, $sid);
$coins             = Utils::sanitaze_var($_POST['coins'], $link, $sid);
$email             = Utils::sanitaze_var($_POST['email'], $link, $sid);
$mIP               = Utils::sanitaze_var($_POST['uIP'], $link, $sid);
$hash              = Utils::sanitaze_var($_POST['hash'], $link, $sid);
$multiemail        = Utils::sanitaze_var($_POST['multiemail'], $link, $sid);
$emailVerification = Utils::sanitaze_var($_POST['emailVerification'], $link, $sid);

if (isset($email)) {
    $email = Utils::sanitaze_var($email, $link);
}
if (isset($mIP)) {
    $mIP = Utils::sanitaze_var($mIP, $link);
}

if (isset($email)) {
    if ($multiemail == "0" && $emailVerification == 0) {
        $emailcount = mysqli_query($link, "SELECT * FROM " . PLAYERS_DB . " WHERE email='$email'");
        if (mysqli_num_rows($emailcount) != 0) {
            die("005"); //already exist email
        }
    }
} else {
    $email = "";
}

$real_hash = Utils::get_secret_hash($name . $password);
if ($real_hash != $hash)
{
    http_response_code(401);
    exit();
}
    $result  = mysqli_query($link, "SELECT * FROM " . PLAYERS_DB . " WHERE name='$name'");
    $numrows = mysqli_num_rows($result);
    mysqli_free_result($result);
    
    //if the login name is already used
    if ($numrows != 0) {
        die("003");
    }
    
    $result   = mysqli_query($link, "SELECT * FROM " . PLAYERS_DB . " WHERE nick='$nick'");
    $numrows2 = mysqli_num_rows($result);
    mysqli_free_result($result);
    
    //if the nickname is already taken
    if ($numrows2 != 0) {
        die("008");
    }
    
    $password = password_hash($password, PASSWORD_BCRYPT, array('cost'=>10));
    $random_hash = "";
    if ($emailVerification == 0) {
        $random_hash = md5(uniqid(rand()));
    }
    
    $result = mysqli_query($link, "INSERT INTO  " . PLAYERS_DB . " (`name` , `nick` , `password` , `ip`, `email`, `verify`, `active`, `coins` ) VALUES ('" . $name . "' ,  '" . $nick . "' ,  '" . $password . "' ,  '" . $mIP . "',  '" . $email . "',  '" . $random_hash . "',  '" . $emailVerification . "',  '" . $coins . "') ") or die(mysqli_error($link));
    
    if ($result) {
        if ($emailVerification == 0) {
            //send verification email          
            $to      = $email;
            $subject = "Activation Code for your " . GAME_NAME . " account";
            $from    = ADMIN_EMAIL;
            $burl = Utils::get_current_file_url();
            $body    = 'Hi ' . $name . '<br/>Your Account has been create, to sign in please verify your email.<br/> <br/> Please Click On This link or paste in your browser: <a href="' . $burl . 'Activation.php?code=' . $random_hash . '">' . $burl . 'Activation.php?=' . $random_hash . '</a> to activate  your account.';
            $headers = "From:" . $from . "\r\n";
            $headers .= "Reply-To: " . $from . "\r\n";
            $headers .= "MIME-Version: 1.0\r\n";
            $headers .= "Content-Type: text/html; charset=ISO-8859-1\r\n";
            $sendemail = mail($to, $subject, $body, $headers);

            if ($sendemail) {
                die("success");
            } else {
                die("006"); //email not send
            }
        } else {
            die("success");
        }
    }
mysqli_close($link);
?>