FROM mcr.microsoft.com/mssql/server:2019-latest

USER root

COPY setup.sql setup.sql
COPY import_data.sh import_data.sh
COPY entrypoint.sh entrypoint.sh

RUN chmod +x import_data.sh
RUN chmod +x entrypoint.sh

ENTRYPOINT [ "/bin/bash", "-c", "./entrypoint.sh" ]