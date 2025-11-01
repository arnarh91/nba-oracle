param ([switch]$DropFirst = $false)

$cs = "server=localhost,14333;database=nba;user id=sa;password=Password12;TrustServerCertificate=true"
$acs = "server=localhost,14333;database=master;user id=sa;password=Password12;TrustServerCertificate=true"

docker build -t nba-oracle:latest -f Dockerfile .

if ($DropFirst) {
    docker run --rm --network=host nba-oracle:latest --silent -w --env "LOCAL" -ct 60 -c "$cs" -acs "$acs" --drop
}
else {
    docker run --rm --network=host nba-oracle:latest --silent -w -t --env "LOCAL" -ct 60 -c "$cs" -acs "$acs"
}