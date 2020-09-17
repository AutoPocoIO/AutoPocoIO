﻿using System.Runtime.CompilerServices;
using System.Web;

#if NETFULL
[assembly: PreApplicationStartMethod(typeof(AutoPoco.DependencyInjection.PreApplicationStart), nameof(AutoPoco.DependencyInjection.PreApplicationStart.Start))]
#endif


[assembly: InternalsVisibleTo("AutoPocoIO.test, PublicKey=0024000004800000940000000602000000240000525341310004000001000100d17d2e43712459ca095519b0017382b866a7061c4089d814c4a4183bb94bfb50de3d45ded820df00470abae5c4ba034ebbab1c88a1bd9a870575c23d3ca87c6cae9fe936f3fb5f55984d0daa63bc0b2353505626bc2b01e077bda217aa6212baa621e4e1ad427e7b0b3c3ed94e4e9b58e4449c803297d41850bfb3d85ce867d5")]
[assembly: InternalsVisibleTo("AutoPocoIO.AspNet.test, PublicKey=0024000004800000940000000602000000240000525341310004000001000100d17d2e43712459ca095519b0017382b866a7061c4089d814c4a4183bb94bfb50de3d45ded820df00470abae5c4ba034ebbab1c88a1bd9a870575c23d3ca87c6cae9fe936f3fb5f55984d0daa63bc0b2353505626bc2b01e077bda217aa6212baa621e4e1ad427e7b0b3c3ed94e4e9b58e4449c803297d41850bfb3d85ce867d5")]
[assembly: InternalsVisibleTo("AutoPocoIO.AspNetCore.test, PublicKey=0024000004800000940000000602000000240000525341310004000001000100d17d2e43712459ca095519b0017382b866a7061c4089d814c4a4183bb94bfb50de3d45ded820df00470abae5c4ba034ebbab1c88a1bd9a870575c23d3ca87c6cae9fe936f3fb5f55984d0daa63bc0b2353505626bc2b01e077bda217aa6212baa621e4e1ad427e7b0b3c3ed94e4e9b58e4449c803297d41850bfb3d85ce867d5")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]