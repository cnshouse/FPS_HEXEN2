<?php
include("bl_Common.php");

$name     = safe($_POST['name']);
$email    = safe($_POST['email']);
$key      = safe($_POST['key']);
$password = safe($_POST['password']);
$hash     = safe($_POST['hash']);
$step     = safe($_POST['step']);

$link = dbConnect();

$name     = stripslashes($name);
$name     = mysqli_real_escape_string($link, $name);
$email    = stripslashes($email);
$email    = mysqli_real_escape_string($link, $email);
$hash     = stripslashes($hash);
$hash     = mysqli_real_escape_string($link, $hash);
$key      = stripslashes($key);
$key      = mysqli_real_escape_string($link, $key);
$password = stripslashes($password);
$password = mysqli_real_escape_string($link, $password);

if ($step == "1") {
    
    if (!filter_var($email, FILTER_VALIDATE_EMAIL)) // Validate email address
        {
        die("004"); //invalid email
    }
    
    $real_hash = md5($email . $secretKey);
    if ($real_hash == $hash) {
        $check = mysqli_query($link, "SELECT * FROM MyGameDB WHERE `name` ='$name' AND `email` ='$email'  ") or die(mysqli_connect_error());
        $numrows = mysqli_num_rows($check);
        
        if ($numrows == 0) {
            die("009"); //user or email not exist
        } else {
            //send verification email           
            $to      = $email;
            $subject = "Reset password request";
            $from    = $emailFrom;
            $body    = 'Hi ' . $name . '<br/><br/>You receive this email because you or someone pretending to be you asked for a password change due forgetting it,  if not you who asked it, do not be alarmed,<br/> without the code below your password can not be changed,\n but if you receive this message multiple times please contact the administrator of the game,<br/> if it has been you simply copy the code below and enter it in the game to make the password change.<br/><br/> <b>Reset Key:</b> ' . $key . '<br/><br/>This key will only be valid during this session of the game.';
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
        }
        
    } else {
        die("You don't have permission for this!");
    }
} else if ($step == "2") {
    $real_hash = md5($name . $secretKey);
    if ($real_hash == $hash) {
        
        $check = mysqli_query($link, "SELECT * FROM MyGameDB WHERE `name` ='$name' ") or die(mysqli_connect_error());
        $numrows = mysqli_num_rows($check);
        
        if ($numrows == 0) {
            die("008"); //user not found
        } else {
            $password = md5($password);
            $update = mysqli_query($link, "UPDATE MyGameDB SET password='" . $password . "' WHERE name='$name'") or die(mysqli_connect_error());
            echo "success";
        }
    } else {
        die("You don't have permission for this!");
    }
}

mysqli_close($link);
?>