<?php
require_once('bl_Common.php');
$type  = $_POST['type'];
$state = $_POST['state'];
$url   = $_POST['url'];
$db_name   = $_POST['dbname'];

if ($type == 0)
  {
    $ch = curl_init();
    curl_setopt($ch, CURLOPT_USERAGENT, "Mozilla/5.0 (Windows; U; Windows NT 5.1; rv:1.7.3) Gecko/20041001 Firefox/0.10.1");
    curl_setopt($ch, CURLOPT_POST, 1);
    curl_setopt($ch, CURLOPT_URL, $url);
    curl_setopt($ch, CURLOPT_FOLLOWLOCATION, true);
    curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
    curl_setopt($ch, CURLOPT_HTTPHEADER, array(
        'Content-Type: application/x-www-form-urlencoded',
        'Content-Length: 0'
    ));
    curl_setopt($ch, CURLOPT_CONNECTTIMEOUT, $timeout);
    curl_setopt($ch, CURLOPT_TIMEOUT, $timeout);
    curl_setopt($ch, CURLOPT_MAXREDIRS, 10);
    $content = curl_exec($ch);
    echo $content;
  }
else if ($type == 1)
  {    
    $db  = Utils::get_sqlite_db('fbSessionsdb.db', 'fbSessions');
    $res = $db->query("SELECT * FROM fbSessions WHERE state = '$state'");
    if ($res)
      {
        if ($res->columnType(0) == SQLITE3_NULL)
          {
            $db->close();
            die("not found");
          }
        while ($row = $res->fetchArray())
          {
            echo "success|{$row['state']}|{$row['code']}";
          }
        $db->query("DELETE FROM fbSessions WHERE state = '$state'");
      }
    else
      {
        $db->lastErrorMsg();
      }
    $db->close();
    
  }
else if ($type == 2)
  {
    $db  = Utils::get_sqlite_db('gSessionsdb.db', 'gSessions');
    $res = $db->query("SELECT * FROM gSessions WHERE state = '$state'");
    if ($res)
      {
        if ($res->columnType(0) == SQLITE3_NULL)
          {
            $db->close();
            die("not found");
          }
        while ($row = $res->fetchArray())
          {
            echo "success|{$row['state']}|{$row['code']}";
          }
        $db->query("DELETE FROM gSessions WHERE state = '$state'");
      }
    else
      {
        $db->lastErrorMsg();
      }
    $db->close();
  }
  else if ($type == 3)
  {
    $db  = Utils::get_sqlite_db($db_name . '.db', $db_name);
    $res = $db->query("SELECT * FROM $db_name WHERE state = '$state'");
    if ($res)
      {
        if ($res->columnType(0) == SQLITE3_NULL)
          {
            $db->close();
            die("not found");
          }
        while ($row = $res->fetchArray())
          {
            echo "success|{$row['state']}|{$row['code']}";
          }
        $db->query("DELETE FROM $db_name WHERE state = '$state'");
      }
    else
      {
        $db->lastErrorMsg();
      }
    $db->close();
  }
?>