FROM public.ecr.aws/lambda/dotnet:7
WORKDIR /var/task
RUN yum clean packages
RUN yum clean headers
RUN yum clean metadata
RUN yum clean all
#RUN yum install -y amazon-linux-extras 
#RUN amazon-linux-extras install epel -y
RUN yum install -y wget
RUN wget https://cdn.amazonlinux.com/2/core/2.0/x86_64/6b0225ccc542f3834c95733dcf321ab9f1e77e6ca6817469771a8af7c49efe6c/../../../../../blobstore/9c2386f3c4202291dcc3c8a9a907e674a7bee0d06cd81801b3ae01a8787120e4/libjpeg-turbo-2.0.90-2.amzn2.0.1.x86_64.rpm
RUN yum install -y libjpeg-turbo-2.0.90-2.amzn2.0.1.x86_64.rpm
RUN yum install -y libXrender
RUN yum install -y libXext
COPY "bin/Release/lambda-publish"  .