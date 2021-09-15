FROM mongo:4.2

COPY ./entrypoint.sh .
RUN chmod +x ./entrypoint.sh

ENTRYPOINT [ "./entrypoint.sh" ]