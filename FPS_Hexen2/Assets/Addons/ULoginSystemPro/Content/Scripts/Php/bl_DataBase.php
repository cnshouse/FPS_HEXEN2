<?php
include("bl_Common.php");

$name     = safe($_POST['name']);
$nick     = safe($_POST['nick']);
$kills    = safe($_POST['kills']);
$deaths   = safe($_POST['deaths']);
$score    = safe($_POST['score']);
$coins    = safe($_POST['coins']);
$nIP      = safe($_POST['nIP']);
$hash     = safe($_POST['hash']);
$typ      = safe($_POST['typ']);
$playTime = safe($_POST['playTime']);
$flist    = safe($_POST['flist']);
$key    = safe($_POST['key']);
$data    = safe($_POST['data']);

$link = dbConnect();

$name     = stripslashes($name);
$name     = mysqli_real_escape_string($link, $name);
$nick     = stripslashes($nick);
$nick     = mysqli_real_escape_string($link, $nick);
$kills    = stripslashes($kills);
$kills    = mysqli_real_escape_string($link, $kills);
$deaths   = stripslashes($deaths);
$deaths   = mysqli_real_escape_string($link, $deaths);
$score    = stripslashes($score);
$score    = mysqli_real_escape_string($link, $score);
$typ      = mysqli_real_escape_string($link, $typ);
$nIP      = stripslashes($nIP);
$nIP      = mysqli_real_escape_string($link, $nIP);
$playTime = stripslashes($playTime);
$playTime = mysqli_real_escape_string($link, $playTime);
$flist    = stripslashes($flist);
$flist    = mysqli_real_escape_string($link, $flist);
$coins    = stripslashes($coins);
$coins    = mysqli_real_escape_string($link, $coins);
$key    = stripslashes($key);
$key    = mysqli_real_escape_string($link, $key);
$data    = stripslashes($data);

$real_hash = md5($name . $secretKey);
if ($real_hash == $hash) {
    if ($typ == "1") { //Save player data
        if ($check = mysqli_query($link, "UPDATE MyGameDB SET kills='" . mysqli_real_escape_string($link, $kills) . "', deaths='" . mysqli_real_escape_string($link, $deaths) . "', score='" . mysqli_real_escape_string($link, $score) . "' WHERE name='$name'")) {
            echo "success";
        } else {
            die(mysqli_error($link));
        }
    } else if ($typ == "2") { //change IP
        $check = mysqli_query($link, "UPDATE MyGameDB SET uIP='" . mysqli_real_escape_string($link, $nIP) . "' WHERE name='$name'") or die(mysqli_connect_error());
        if ($check) {
            echo "successip";
        }
    } else if ($typ == "3") { //play time update
        $lastp = mysqli_query($link, "SELECT playtime FROM MyGameDB WHERE name='$name'") or die(mysqli_connect_error());
        $lastone    = mysqli_fetch_assoc($lastp);
        $actualTime = (int) $lastone['playtime'];
        $actualTime += $playTime;
        if ($check = mysqli_query($link, "UPDATE MyGameDB SET playtime='" . mysqli_real_escape_string($link, $actualTime) . "' WHERE name='$name'")) {
            echo "successpt";
        } else {
            die(mysqli_connect_error());
        }
        
    } else if ($typ == "4") { //change nick name
        $check2   = mysqli_query($link, "SELECT * FROM MyGameDB WHERE `nick`= '$nick'");
        $numrows2 = mysqli_num_rows($check2);
        if ($numrows2 == 0) {
            if (mysqli_query($link, "UPDATE MyGameDB SET nick='" . mysqli_real_escape_string($link, $nick) . "' WHERE name='$name'")) {
                echo "successcn";
            }
        } else {
            die("008"); // nick name already exist
        }
    } else if ($typ == "5") { //save friend list
        $check2   = mysqli_query($link, "SELECT * FROM MyGameDB WHERE `name`= '$name'");
        $numrows2 = mysqli_num_rows($check2);
        if ($numrows2 != 0) {
            if (mysqli_query($link, "UPDATE MyGameDB SET flist='" . $flist . "' WHERE name='$name'")) {
                echo "save";
            }
        } else {
            die("008"); // player with this name not exist
        }
    } else if ($typ == "6") { //save coins
        $check2   = mysqli_query($link, "SELECT * FROM MyGameDB WHERE `id`= '$name'") or die(mysqli_error($link));
        $numrows2 = mysqli_num_rows($check2);
        if ($numrows2 != 0) {
            if (mysqli_query($link, "UPDATE MyGameDB SET coins='" . $coins . "' WHERE id='$name'")) {
                echo "save";
            }
        } else {
            die("008"); // player with this name not exist
        }
    }else if($typ == "7")//Save coin purchase
    {
        $check = mysqli_query($link, "SELECT * FROM MyGameDB WHERE `id`= '$name'") or die(mysqli_error($link));
        $numrows = mysqli_num_rows($check);
        if ($numrows >= 1) {
            $rowData = mysqli_fetch_assoc($check);
            $json = json_decode($data, true);

              (int)$totalCoins = $rowData["coins"] + $json["coins"];
                $result = mysqli_query($link, "UPDATE MyGameDB SET coins='{$totalCoins}' WHERE id='$name'") or die("Error: " . mysqli_error($link));
            if ($result) {
                $result = mysqli_query($link,"INSERT INTO Purchases (productID, receipt, userID) Values ('{$json["productID"]}', '{$json["receipt"]}', '{$name}')") or die(mysqli_error($link));
                if($result){
                EchoWithPrefix($totalCoins);
                }
            }
        } else {
            die("Could's find player: ". $name); // player with this name not exist
        }
    }
    else if($typ == 8)//Update a give pair data (value and key from POST)
    {
        $check = mysqli_query($link, "SELECT * FROM MyGameDB WHERE `id`= '$name'") or die(mysqli_error($link));
        $numrows = mysqli_num_rows($check);
        if ($numrows >= 1) {
           
            $result = mysqli_query($link, "UPDATE MyGameDB SET $key='" . $data . "' WHERE id='$name'") or die(mysqli_error($link));
            if ($result) {
                echo "save";
            }

        } else {
            die("Could's find player: ". $name); // player with this name not exist
        }
    } else {
        die("Any type are assigned with this id:" . $typ . " for user: " . $name);
    }
    
} else {
    die("You don't have permission for this!");
}

mysqli_close($link);
?> 
<?php
function GetColumn($link, $user, $cname)
{
    $check = mysqli_query($link, "SELECT * FROM MyGameDB WHERE `name` ='$user' ") or die(mysqli_connect_error());
    $numrows = mysqli_num_rows($check);
    
    if ($numrows == 0) {
        die("001"); //user not exist
    } else {
        while ($row = mysqli_fetch_assoc($check)) {
            return $row[$cname];
        }
    }
}
?>