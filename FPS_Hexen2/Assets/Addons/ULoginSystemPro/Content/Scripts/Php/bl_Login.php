<?php
include_once("bl_Common.php");
Utils::check_session($_POST['sid']);

$link = Connection::dbConnect();

$sid     = Utils::sanitaze_var($_POST['sid'], $link);
$name    = Utils::sanitaze_var($_POST['name'], $link, $sid);
$pass    = Utils::sanitaze_var($_POST['password'], $link, $sid);
$authApp = Utils::sanitaze_var($_POST['appAuth'], $link, $sid);

if (empty($name))
  {
    http_response_code(400);
    exit();
  }

$query   = Connection::Query($link, "SELECT * FROM " . PLAYERS_DB . " WHERE `name` ='$name' ");
$numrows = mysqli_num_rows($query);

if ($numrows == 0)
  {
    http_response_code(401);
    exit();
  }

while ($row = mysqli_fetch_assoc($query))
  {
      if(password_verify($pass, $row['password']))
      {
        if ($row['active'] == "1" || $authApp != "ulogin")
          {
            PrintData($row, $sid);
          }
        else
          {
            die("007");
          }
      }
    else
      {
        http_response_code(401);
      }
  }
mysqli_close($link);

function PrintData($row, $sid)
  {
    $data = "success\n";
    foreach ($row as $key => $value)
      {
        if ($key == "password") //don't retrieve the password
            continue;
            $data .= $key . "|" . $value . "\n";
      }
      if(PER_TO_PER_ENCRYPTION)
      {
        $data = "encrypt" . Utils::encrypt_aes($data,$sid);
      }
      echo $data;
  }
?>