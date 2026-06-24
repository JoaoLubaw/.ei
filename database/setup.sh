SCRIPTS_DIR="${SCRIPTS_DIR:-scripts}"

if [ ! -d "$SCRIPTS_DIR" ]; then
    echo "'$SCRIPTS_DIR' not found!"
    exit 1
fi

if [ -z "$POSTGRES_USER" ]; then
    echo "Error: POSTGRES_USER is not set!"
    exit 1
fi

if [ -z "$POSTGRES_PASSWORD" ]; then
    echo "Error: POSTGRES_PASSWORD is not set!"
    exit 1
fi

if [ -z "$PONTUEI_PASSWORD" ]; then
    echo "Error: PONTUEI_PASSWORD is not set!"
    exit 1
fi

cd "$SCRIPTS_DIR"

export PGPASSWORD=${POSTGRES_PASSWORD}
export PGOPTIONS="-c client_min_messages=notice"

# P01 
for f in P01*.sql; do
    echo "$f"
    psql -v ON_ERROR_STOP=1 \
         -U "${POSTGRES_USER}" \
         -v pontuei_password="${PONTUEI_PASSWORD}" \
         -f "$f"
done

# P02 
export PGPASSWORD=${PONTUEI_PASSWORD}

for f in P02*.sql; do
    echo "$f"
    psql -v ON_ERROR_STOP=1 -U pontuei -d pontuei -f "$f"
done

for f in P03*.sql; do
    echo "$f"
    psql -v ON_ERROR_STOP=1 -U pontuei -d pontuei -f "$f"
done

for f in P04*.sql; do
    echo "$f"
    psql -v ON_ERROR_STOP=1 -U pontuei -d pontuei -f "$f"
done