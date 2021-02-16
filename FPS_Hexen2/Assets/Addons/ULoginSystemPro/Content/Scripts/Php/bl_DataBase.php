<?php
include_once("bl_Common.php");
include_once("bl_Functions.php");
Utils::check_session($_POST['sid']);

$link = Connection::dbConnect();

$sid      = Utils::sanitaze_var($_POST['sid'], $link);
$name     = Utils::sanitaze_var($_POST['name'], $link, $sid);
$id       = Utils::sanitaze_var($_POST['id'], $link, $sid);
$nick     = Utils::sanitaze_var($_POST['nick'], $link, $sid);
$kills    = Utils::sanitaze_var($_POST['kills'], $link, $sid);
$deaths   = Utils::sanitaze_var($_POST['deaths'], $link, $sid);
$score    = Utils::sanitaze_var($_POST['score'], $link, $sid);
$nIP      = Utils::sanitaze_var($_POST['nIP'], $link, $sid);
$type     = Utils::sanitaze_var($_POST['typ'], $link, $sid);
$key      = Utils::sanitaze_var($_POST['key'], $link, $sid);
$values   = Utils::sanitaze_var($_POST['values'], $link, $sid);
$hash     = Utils::sanitaze_var($_POST['hash'], $link, $sid);
$data    = safe($_POST['data']);
$data    = stripslashes($data);

$real_hash = Utils::get_secret_hash($name);
if ($real_hash != $hash) {
    http_response_code(401);
    exit();
}

$functions = new Functions($link);

switch ($type) { //update user data
    case 1: //Save basic player stats
        if($functions->update_user_data($key,$values,'id',$id)) success_response();
        else fail_response();
        break;
    case 2: //Update user IP
        if (mysqli_query($link, "UPDATE " . PLAYERS_DB . " SET ip='" .  $nIP . "' WHERE name='$name'") or die(mysqli_error($link))) {
            echo "successip";
        }
        break;
    case 3: //update player play time
        $sql = "UPDATE " . PLAYERS_DB . " SET playtime=playtime+" . $values . " WHERE id='$id'";
        if($functions->Query($sql)) success_response();
        else fail_response();
        break;
    case 4: //Check if an user exist
            if($functions->user_exist_custom($key,$values)) success_response();
            else fail_response();
            break;
    case 6: //Update user coins
        $coins = (int)$values;
        $sql = "UPDATE " . PLAYERS_DB . " SET coins=coins+" . $coins . " WHERE id='$id'";
        if($key == 2){ $sql = "UPDATE " . PLAYERS_DB . " SET coins=coins-" . $coins . " WHERE id='$id'";}
        if($functions->Query($sql))
        {
          $newCoins = $functions->get_user_row($id,'coins');
          Response($newCoins,$sid);
        }else fail_response();
        break;
    case 7: //Save coins purchase
          $purchase_info = json_decode($data, true);
          $sql = "UPDATE " . PLAYERS_DB . " SET coins=coins+" . $purchase_info["coins"] . " WHERE id='$id';";
          $sql .= "INSERT INTO " . PURCHASES_DB . " (product_id, receipt, user_id) Values ('{$purchase_info["productID"]}', '{$purchase_info["receipt"]}', '{$id}')";
          if($functions->multiple_query($sql))
          {
            $newCoins = $functions->get_user_row($id,'coins');
            Response($newCoins,$sid);
          }else { fail_response(); }
        break;
    case 8: //Update a give pair data (value and key from POST)
        if($functions->update_user_row($key,$data,'id',$id)) success_response();
        else fail_response();
        break;
    default:
        http_response_code(400);
        exit();
        break;
}
mysqli_close($link);
?> 