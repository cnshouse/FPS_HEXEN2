<?php
include_once("bl_Common.php");
include_once("bl_Functions.php");
Utils::check_session($_POST['sid']);

$link = Connection::dbConnect();

$sid   = Utils::sanitaze_var($_POST['sid'], $link);
$name  = Utils::sanitaze_var($_POST['name'], $link, $sid);
$data  = Utils::sanitaze_var($_POST['data'], $link, $sid);
$type  = Utils::sanitaze_var($_POST['type'], $link, $sid);
$ip    = Utils::sanitaze_var($_POST['ip'], $link, $sid);
$autor = Utils::sanitaze_var($_POST['author'], $link, $sid);
$hash  = Utils::sanitaze_var($_POST['hash'], $link, $sid);
$unsafe = $_POST['unsafe'];

$real_hash = Utils::get_secret_hash($name);
if ($real_hash != $hash) {
    http_response_code(401);
    exit();
}

$functions = new Functions($link);

switch ($type) {
    case 1: //Do a ban
        $check = mysqli_query($link, "SELECT * FROM " . BANS_DB . " WHERE `name`= '$name'");
        if (mysqli_num_rows($check) != 0) {
            die("This player is already banned.");
        }
        $query = "INSERT INTO " . BANS_DB . " (`name` ,  `reason` ,  `ip` ,  `by` ) VALUES ('" . $name . "' ,  '" . $data . "' ,  '" . $ip . "',  '" . $autor . "');";
        $query .= "UPDATE " . PLAYERS_DB . " SET status='3' WHERE name= '$name'";

        if($functions->multiple_query($query))
        {
            echo "success";
        }
        break;
    case 2: //UnBan a player
        $sql       = "DELETE FROM " . BANS_DB . " WHERE name= '$name';";
        $sql .= "UPDATE " . PLAYERS_DB . " SET status='0' WHERE name= '$name'";
        
        if (mysqli_multi_query($link, $sql) or die(mysqli_error($link))) {
            echo "success";
        }
        break;
    case 3: //Change player status
        $query = "UPDATE " . PLAYERS_DB . " SET status='" . $data . "' WHERE name='$name'";
       if(mysqli_query($link,$query) or  die(mysqli_error($link))){
        echo "success";
       }
        break;
    case 4: //search user by their name and get their basic stats
        $result = $functions->get_user_by('name',$name);
        if(!$result)
        {          
            $result = $functions->get_user_by('nick',$name);
            if(!$result)
            {
                die("User with this name or nick name does not exist in DataBase!");
            }
            echo FetchBasicStats($result);
        }else
        {
            echo FetchBasicStats($result);
        }
        break;
    case 5: //update player values given from the client side
        $check = mysqli_query($link, "SELECT * FROM " . PLAYERS_DB . " WHERE id='$name'") or die(mysqli_error($link));
        
        if (mysqli_num_rows($check) == 0) {
            die("Player " . $name . " not found.");
        }
        $query = mysqli_query($link, "UPDATE " . PLAYERS_DB . " SET " . $unsafe . " WHERE id='$name'") or die(mysqli_error($link));
        if ($query) {
            echo "done";
        }
        break;
        case 6://Get database stats
            $result = mysqli_query($link, "SELECT count(*) as total from " . PLAYERS_DB);
            $data = mysqli_fetch_assoc($result);
            $tablecount = $data['total'];
            $result2 = mysqli_query($link, "SELECT count(*) as total from " . BANS_DB );
            $data2 = mysqli_fetch_assoc($result2);
            $tablecount2 = $data2['total'];
            $result3 = mysqli_query($link, "SELECT SUM(playtime) as total from " . PLAYERS_DB);
            $data3 = mysqli_fetch_assoc($result3);
            $tablecount3 = $data3['total'];
            $lastp = mysqli_query($link, "SELECT nick FROM " . PLAYERS_DB . " ORDER BY `id` DESC LIMIT 1") or die(mysqli_error($link));
            $lastone = mysqli_fetch_assoc($lastp);
            echo "success|" . $tablecount . "|" . $lastone["nick"] . "|" . $tablecount2 . "|" . $tablecount3;
        break;
}
mysqli_close($link);

function FetchBasicStats($query)
{
    $stats = "";
    while ($row = mysqli_fetch_assoc($query)) {
        $stats = "success|" . $row['name'] . "|" . $row['kills'] . "|" . $row['deaths'] . "|" . $row['score'] . "|" . $row['ip'] . "|" . $row['status'] . "|" . $row['playtime'] . "|" . $row['nick'] . "|" . $row['id'] . "|";
    }
    return $stats;
}
?>