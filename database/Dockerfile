FROM postgres:17

COPY scripts/ /scripts/
COPY setup.sh /docker-entrypoint-initdb.d/
RUN chmod +x /docker-entrypoint-initdb.d/setup.sh