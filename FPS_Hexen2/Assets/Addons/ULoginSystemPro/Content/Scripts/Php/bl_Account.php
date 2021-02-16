<?php
include_once("bl_Common.php");
include_once("bl_Functions.php");
Utils::check_session($_POST['sid']);

$link = Connection::dbConnect();

$sid      = Utils::sanitaze_var($_POST['sid'], $link);
$userId      = Utils::sanitaze_var($_POST['id'], $link, $sid);
$type        = Utils::sanitaze_var($_POST['type'], $link, $sid);
$password    = Utils::sanitaze_var($_POST['password'], $link, $sid);
$data        = Utils::sanitaze_var($_POST['data'], $link, $sid);
$email       = Utils::sanitaze_var($_POST['email'], $link, $sid);
$hash        = Utils::sanitaze_var($_POST['hash'], $link, $sid);

$real_hash = Utils::get_secret_hash($userId);
if ($real_hash != $hash) {
    http_response_code(401);
    exit();
}

$functions = new Functions($link);
    
switch($type)
{
    case 1: //Change account password
        if(!$functions->user_exist($userId))
        {
            die("008"); //user not found
        }

        $curren_pass = $functions->get_user_row($userId,'password');
        $password = md5($password);
        if($password == $curren_pass)
        {
            $data = md5($data);
            if($functions->update_user_row('password',$data,'id',$userId))success_response();
            else fail_response();           
        }else  die("002"); //wrong password
    break;
    case 2: //Send reset password email confirmation
        if (!filter_var($email, FILTER_VALIDATE_EMAIL)) // Validate email address
        {
        die("004"); //invalid email
        }

        $check = mysqli_query($link, "SELECT * FROM " . PLAYERS_DB . " WHERE `name` ='$userId' AND `email` ='$email'") or die(mysqli_error($link));    
        if ($mysqli_num_rows($check) == 0) {
            die("009"); //user or email not exist or not match
        } else {
            //send verification email           
            $to      = $email;
            $subject = "Reset password request";
            $from    = ADMIN_EMAIL;
            $body    = 'Hi ' . $userId . '<br/><br/>You receive this email because you or someone pretending to be you asked for a password change due forgetting it,  if not you who asked it, do not be alarmed,<br/> without the code below your password can not be changed,\n but if you receive this message multiple times please contact the administrator of the game,<br/> if it has been you simply copy the code below and enter it in the game to make the password change.<br/><br/> <b>Reset Key:</b> ' . $data . '<br/><br/>This key will only be valid during this session of the game.';
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
    break;
    case 3://Change account password
        $check = mysqli_query($link, "SELECT * FROM " . PLAYERS_DB . " WHERE `name` ='$userId' ") or die(mysqli_error($link));     
        if (mysqli_num_rows($check) == 0) {
            die("008"); //user not found
        } else {
            $password = md5($password);
            $update = mysqli_query($link, "UPDATE " . PLAYERS_DB . " SET password='" . $password . "' WHERE name='$userId'") or die(mysqli_error($link));
            echo "success";
        }
    break;
    case 4://Change account nickname
        $result   = mysqli_query($link, "SELECT * FROM " . PLAYERS_DB . " WHERE `nick`= '$data'") or die(mysqli_error($link));
        if (mysqli_num_rows($result) == 0) {
            if (mysqli_query($link, "UPDATE " . PLAYERS_DB . " SET nick='" .  $data . "' WHERE name='$userId'")) {
                echo "success";
            }
        } else {
            die("008"); // nick name already exist
        }
    break;
}
mysqli_close($link);
?>