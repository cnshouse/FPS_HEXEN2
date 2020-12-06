<?php
include("bl_Common.php");
$link = dbConnect();

$name              = safe($_POST['name']);
$nick              = safe($_POST['nick']);
$password          = safe($_POST['password']);
$kills             = safe($_POST['kills']);
$deaths            = safe($_POST['deaths']);
$score             = safe($_POST['score']);
$coins             = safe($_POST['coins']);
$email             = $_POST['email'];
$mIP               = $_POST['uIP'];
$hash              = safe($_POST['hash']);
$multiemail        = safe($_POST['multiemail']);
$emailVerification = safe($_POST['emailVerification']);

if (isset($email)) {
$email    = stripslashes($email);
$email    = mysqli_real_escape_string($link, $email);
}
$name     = stripslashes($name);
$name     = mysqli_real_escape_string($link, $name);
$nick     = stripslashes($nick);
$nick     = mysqli_real_escape_string($link, $nick);
$password = stripslashes($password);
$password = mysqli_real_escape_string($link, $password);
$mIP      = stripslashes($mIP);
$mIP      = mysqli_real_escape_string($link, $mIP);
$coins    = stripslashes($coins);
$coins    = mysqli_real_escape_string($link, $coins);


if (isset($email)) {
    if ($multiemail == "0" && $emailVerification == "0") {
        $emailcount = mysqli_query($link, "SELECT * FROM MyGameDB WHERE email='$email'");
        if (mysqli_num_rows($emailcount) != 0) {
            die("005"); //already exist email
        }
    }
}

$real_hash = md5($name . $password . $secretKey);
if ($real_hash == $hash) {
    $check   = mysqli_query($link, "SELECT * FROM MyGameDB WHERE name='$name'");
    $numrows = mysqli_num_rows($check);
    
    if ($numrows == 0) {
        
        $check2   = mysqli_query($link, "SELECT * FROM MyGameDB WHERE nick='$nick'");
        $numrows2 = mysqli_num_rows($check2);
        if ($numrows2 == 0) {
            
            $password    = md5($password);
            $random_hash = md5(uniqid(rand()));
            
            $ins = mysqli_query($link, "INSERT INTO  `MyGameDB` (`name` , `nick` , `password` , `uIP`, `email`, `verify`, `active`, `coins` ) VALUES ('" . $name . "' ,  '" . $nick . "' ,  '" . $password . "' ,  '" . $mIP . "',  '" . $email . "',  '" . $random_hash . "',  '" . $emailVerification . "',  '" . $coins . "') ") or die(mysqli_error($link));
            
            if ($ins) {
                if ($emailVerification == "0") {
                    //send verification email           
                    $to      = $email;
                    $subject = "Activation Code For " . $GameName;
                    $from    = $emailFrom;
                    $body    = 'Hi ' . $name . '<br/>Your Account has been create, to sign in please verify your email.<br/> <br/> Please Click On This link or paste in your browser: <a href="' . $base_url . 'Activation.php?code=' . $random_hash . '">' . $base_url . 'Activation.php?=' . $random_hash . '</a> to activate  your account.';
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
            } else {
                die("Query Error: " . mysqli_error($link));
            }
        } else {
            die("008"); //user nick name exist
        }
    } else {
        die("003"); //user exist
    }
} else {
    die("You don't have permission for this");
}
mysqli_close($link);
?>